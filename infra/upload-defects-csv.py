#!/usr/bin/env python3
"""
Script for uploading defects CSV data to Azure Cosmos DB
with fallback authentication.
Tries Azure AD first, then falls back to access keys if available.
"""

import csv
import json
import os
import uuid
from datetime import datetime
from azure.cosmos import CosmosClient
from azure.cosmos.exceptions import (
    CosmosResourceExistsError,
    CosmosResourceNotFoundError
)
from azure.identity import DefaultAzureCredential
from dotenv import load_dotenv

# Load environment variables from .env file
load_dotenv()


def connect_to_cosmos():
    """
    Connect to Azure Cosmos DB using Azure AD or access key authentication.
    Tries Azure AD first, then falls back to access keys.
    """
    
    endpoint = os.environ.get('COSMOS_ENDPOINT')
    if not endpoint:
        endpoint = input("Enter Cosmos DB Endpoint: ")
    
    # Try Azure AD authentication first
    try:
        print("🔐 Attempting Azure AD authentication...")
        credential = DefaultAzureCredential()
        client = CosmosClient(endpoint, credential)
        # Test the connection
        list(client.list_databases())
        print(f"✅ Connected to Cosmos DB at {endpoint} using Azure AD")
        return client
    except Exception as aad_error:
        print(f"⚠️  Azure AD authentication failed: {aad_error}")
        
    # Fallback to access key authentication
    try:
        print("🔑 Falling back to access key authentication...")
        key = os.environ.get('COSMOS_PRIMARY_KEY')
        if not key:
            key = input("Enter Cosmos DB Primary Key: ")
        
        client = CosmosClient(endpoint, key)
        # Test the connection
        list(client.list_databases())
        print(f"✅ Connected to Cosmos DB at {endpoint} using access key")
        return client
    except Exception as key_error:
        print(f"❌ Access key authentication also failed: {key_error}")
        print("\n💡 To fix this issue:")
        print("1. For Azure AD: Grant yourself 'Cosmos DB Built-in Data Contributor' role")
        print("2. For access keys: Enable key-based authentication on the Cosmos account")
        print("3. Check that your .env file has the correct COSMOS_ENDPOINT")
        return None


def read_defects_csv(file_path):
    """Read defects CSV data and return as list of dictionaries."""
    
    if not os.path.exists(file_path):
        print(f"❌ CSV file not found: {file_path}")
        return []
    
    defects = []
    try:
        with open(file_path, 'r', encoding='utf-8') as file:
            reader = csv.DictReader(file)
            for row in reader:
                defects.append(row)
        
        print(f"📄 Read {len(defects)} defects from {file_path}")
        return defects
        
    except Exception as e:
        print(f"❌ Error reading CSV file: {e}")
        return []


def transform_defect_data(defect_row):
    """Transform a defect row into the format expected by Cosmos DB."""
    
    # Generate a unique ID for this defect
    defect_id = str(uuid.uuid4())
    
    # Convert numeric fields with proper error handling
    try:
        severity_score = float(defect_row.get('severity_score', 0))
    except (ValueError, TypeError):
        severity_score = 0.0
    
    try:
        repair_cost = float(defect_row.get('repair_cost', 0))
    except (ValueError, TypeError):
        repair_cost = 0.0
    
    # Parse date with error handling
    defect_date = defect_row.get('defect_date', '')
    try:
        # Try to parse the date and convert to ISO format
        parsed_date = datetime.strptime(defect_date, '%Y-%m-%d')
        defect_date_iso = parsed_date.isoformat()
    except (ValueError, TypeError):
        # If parsing fails, use current date
        defect_date_iso = datetime.now().isoformat()
    
    # Create the document structure
    document = {
        "id": defect_id,
        "defectId": defect_row.get('defect_id', ''),
        "productId": defect_row.get('product_id', ''),
        "companyId": defect_row.get('company_id', ''),
        "defectType": defect_row.get('defect_type', ''),
        "defectLocation": defect_row.get('defect_location', ''),
        "severity": defect_row.get('severity', ''),
        "severityScore": severity_score,
        "defectDate": defect_date_iso,
        "inspectionMethod": defect_row.get('inspection_method', ''),
        "repairCost": repair_cost,
        "dataSource": "manufacturing_defects_csv",
        "importTimestamp": datetime.now().isoformat(),
        "partitionKey": defect_row.get('defect_type', 'unknown')
    }
    
    return document


def create_database_and_container(client, database_name, container_name):
    """Create database and container if they don't exist."""
    
    try:
        # Create database
        database = client.create_database_if_not_exists(id=database_name)
        print(f"✅ Database '{database_name}' ready")
        
        # Create container with partition key (no throughput for serverless)
        container = database.create_container_if_not_exists(
            id=container_name,
            partition_key={'paths': ['/partitionKey'], 'kind': 'Hash'}
            # Note: No offer_throughput for serverless accounts
        )
        print(f"✅ Container '{container_name}' ready")
        
        return container
        
    except Exception as e:
        print(f"❌ Error creating database/container: {e}")
        return None


def upload_defects_to_cosmos(client, defects, database_name='ManufacturingDataDocDB', container_name='repairs'):
    """Upload defects data to Cosmos DB."""
    
    if not defects:
        print("❌ No defects data to upload")
        return False
    
    # Create database and container
    container = create_database_and_container(client, database_name, container_name)
    if not container:
        return False
    
    # Upload documents
    success_count = 0
    error_count = 0
    
    print(f"📤 Starting upload of {len(defects)} defects...")
    
    for i, defect_row in enumerate(defects, 1):
        try:
            document = transform_defect_data(defect_row)
            container.create_item(body=document)
            success_count += 1
            
            if i % 100 == 0:  # Progress update every 100 items
                print(f"   Uploaded {i}/{len(defects)} defects...")
                
        except CosmosResourceExistsError:
            print(f"⚠️  Defect {defect_row.get('defect_id', 'unknown')} already exists, skipping...")
            error_count += 1
        except Exception as e:
            print(f"❌ Error uploading defect {defect_row.get('defect_id', 'unknown')}: {e}")
            error_count += 1
    
    print(f"\n📊 Upload complete!")
    print(f"   ✅ Successfully uploaded: {success_count}")
    print(f"   ❌ Errors/skipped: {error_count}")
    print(f"   📈 Total processed: {len(defects)}")
    
    return success_count > 0


def main():
    """Main function to orchestrate the CSV upload process."""
    
    print("🏭 Manufacturing Defects CSV Upload to Cosmos DB")
    print("=======================================================")
    
    # Connect to Cosmos DB
    client = connect_to_cosmos()
    if not client:
        print("❌ Could not connect to Cosmos DB. Exiting.")
        return
    
    # Read CSV data
    csv_file_path = "data/defects_data_with_company.csv"
    defects = read_defects_csv(csv_file_path)
    
    if not defects:
        print("❌ No data to upload. Exiting.")
        return
    
    # Upload to Cosmos DB
    success = upload_defects_to_cosmos(client, defects)
    
    if success:
        print("\n🎉 Upload completed successfully!")
    else:
        print("\n❌ Upload failed. Check the errors above.")


if __name__ == "__main__":
    main()
