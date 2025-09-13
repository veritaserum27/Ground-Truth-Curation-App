#!/usr/bin/env python3
"""
Robust CSV Import - Handles connection timeouts and large datasets
"""

import csv
import sys
import os
import time
from typing import Dict

try:
    import pyodbc
except ImportError:
    print("❌ pyodbc is not installed. Please install it first:")
    print("   pip install pyodbc")
    sys.exit(1)

try:
    from dotenv import load_dotenv
except ImportError:
    print("❌ python-dotenv is not installed. Please install it first:")
    print("   pip install python-dotenv")
    sys.exit(1)

# Load environment variables from .env file
print("🔍 Loading environment variables...")
script_dir = os.path.dirname(os.path.abspath(__file__))
env_file = os.path.join(script_dir, '.env')
print(f"📁 Looking for .env file at: {env_file}")

if os.path.exists(env_file):
    print("✅ .env file found")
    load_dotenv(env_file)
else:
    print("⚠️  .env file not found, looking in current directory")
    load_dotenv()

# Database connection parameters from environment variables
CONNECTION_PARAMS = {
    'server': os.getenv('DB_SERVER'),
    'database': os.getenv('DB_DATABASE', 'SystemDemoDB'),
    'username': os.getenv('DB_USERNAME'),
    'password': os.getenv('DB_PASSWORD'),
    'driver': os.getenv('DB_DRIVER', '{ODBC Driver 18 for SQL Server}'),
    'port': int(os.getenv('DB_PORT', 1433))
}

# Validate required environment variables
required_vars = ['DB_SERVER', 'DB_USERNAME', 'DB_PASSWORD']
missing_vars = [var for var in required_vars if not os.getenv(var)]

if missing_vars:
    print("❌ Missing required environment variables:")
    for var in missing_vars:
        print(f"   - {var}")
    print("\n📝 Please create a .env file with the required variables.")
    print("   See .env.example for the template.")
    sys.exit(1)

# Debug: Print loaded configuration (masking password)
print("🔧 Database Configuration:")
print(f"   Server: {CONNECTION_PARAMS['server']}")
print(f"   Database: {CONNECTION_PARAMS['database']}")
print(f"   Username: {CONNECTION_PARAMS['username']}")
print(f"   Password: {'*' * len(CONNECTION_PARAMS['password']) if CONNECTION_PARAMS['password'] else 'None'}")
print(f"   Port: {CONNECTION_PARAMS['port']}")
print()


def create_connection_string(params: Dict[str, str]) -> str:
    """Create connection string for SQL Server"""
    return (
        f"DRIVER={params['driver']};"
        f"SERVER={params['server']},{params['port']};"
        f"DATABASE={params['database']};"
        f"UID={params['username']};"
        f"PWD={params['password']};"
        f"Encrypt=yes;"
        f"TrustServerCertificate=no;"
        f"Connection Timeout=30;"
    )


def get_connection():
    """Get a fresh database connection with retry logic"""
    conn_str = create_connection_string(CONNECTION_PARAMS)
    max_retries = 3
    for attempt in range(max_retries):
        try:
            conn = pyodbc.connect(conn_str)
            conn.timeout = 30
            return conn
        except Exception as e:
            if attempt < max_retries - 1:
                print(f"⚠️  Connection attempt {attempt + 1} failed, retrying...")
                time.sleep(2)
            else:
                raise e


def import_csv_chunk(csv_file_path: str, start_row: int = 1,
                     chunk_size: int = 1000):  # Reduced chunk size
    """Import a chunk of CSV data with better error handling"""
    
    if not os.path.exists(csv_file_path):
        print(f"❌ Error: CSV file not found at {csv_file_path}")
        return False, 0, False
    
    conn = None
    cursor = None
    
    try:
        # Get fresh connection
        print(f"🔗 Connecting to database for chunk starting at "
              f"row {start_row}...")
        conn = get_connection()
        cursor = conn.cursor()
        
        # Check current record count
        cursor.execute("SELECT COUNT(*) FROM support_tickets")
        current_count = cursor.fetchone()[0]
        print(f"📊 Current database records: {current_count:,}")
        
        # Prepare MERGE statement to handle duplicates gracefully
        insert_sql = """
        MERGE support_tickets AS target
        USING (VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?,
                      ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)) AS source
        (ticket_id, day_of_week, day_of_week_num, company_id, company_size,
         company_size_cat, industry, industry_cat, customer_tier,
         customer_tier_cat, org_users, region, region_cat, past_30d_tickets,
         past_90d_incidents, product_area, product_area_cat, booking_channel,
         booking_channel_cat, reported_by_role, reported_by_role_cat,
         customers_affected, error_rate_pct, downtime_min, payment_impact_flag,
         security_incident_flag, data_loss_flag, has_runbook,
         customer_sentiment, customer_sentiment_cat, description_length,
         priority, priority_cat)
        ON target.ticket_id = source.ticket_id
        WHEN NOT MATCHED THEN
            INSERT (ticket_id, day_of_week, day_of_week_num, company_id, company_size,
                   company_size_cat, industry, industry_cat, customer_tier,
                   customer_tier_cat, org_users, region, region_cat, past_30d_tickets,
                   past_90d_incidents, product_area, product_area_cat, booking_channel,
                   booking_channel_cat, reported_by_role, reported_by_role_cat,
                   customers_affected, error_rate_pct, downtime_min, payment_impact_flag,
                   security_incident_flag, data_loss_flag, has_runbook,
                   customer_sentiment, customer_sentiment_cat, description_length,
                   priority, priority_cat)
            VALUES (source.ticket_id, source.day_of_week, source.day_of_week_num, 
                   source.company_id, source.company_size, source.company_size_cat, 
                   source.industry, source.industry_cat, source.customer_tier,
                   source.customer_tier_cat, source.org_users, source.region, 
                   source.region_cat, source.past_30d_tickets, source.past_90d_incidents, 
                   source.product_area, source.product_area_cat, source.booking_channel,
                   source.booking_channel_cat, source.reported_by_role, 
                   source.reported_by_role_cat, source.customers_affected, 
                   source.error_rate_pct, source.downtime_min, source.payment_impact_flag,
                   source.security_incident_flag, source.data_loss_flag, 
                   source.has_runbook, source.customer_sentiment, 
                   source.customer_sentiment_cat, source.description_length,
                   source.priority, source.priority_cat);
        """
        
        # Read CSV and process chunk
        with open(csv_file_path, 'r', encoding='utf-8') as file:
            csv_reader = csv.DictReader(file)
            
            # Skip rows until start_row
            current_row = 1
            for _ in range(start_row - 1):
                try:
                    next(csv_reader)
                    current_row += 1
                except StopIteration:
                    print("✅ Reached end of file")
                    return True, 0, True
            
            # Process chunk
            records_processed = 0
            records_imported = 0
            
            for row in csv_reader:
                if records_processed >= chunk_size:
                    break
                    
                try:
                    # Prepare row data
                    row_data = (
                        int(row['ticket_id']),
                        row['day_of_week'],
                        int(row['day_of_week_num']),
                        int(row['company_id']),
                        row['company_size'],
                        int(row['company_size_cat']),
                        row['industry'],
                        int(row['industry_cat']),
                        row['customer_tier'],
                        int(row['customer_tier_cat']),
                        int(row['org_users']),
                        row['region'],
                        int(row['region_cat']),
                        int(row['past_30d_tickets']),
                        int(row['past_90d_incidents']),
                        row['product_area'],
                        int(row['product_area_cat']),
                        row['booking_channel'],
                        int(row['booking_channel_cat']),
                        row['reported_by_role'],
                        int(row['reported_by_role_cat']),
                        int(row['customers_affected']),
                        float(row['error_rate_pct']),
                        int(row['downtime_min']),
                        int(row['payment_impact_flag']),
                        int(row['security_incident_flag']),
                        int(row['data_loss_flag']),
                        int(row['has_runbook']),
                        # Handle empty customer_sentiment as NULL
                        (row['customer_sentiment'].strip()
                         if row['customer_sentiment'].strip() else None),
                        int(row['customer_sentiment_cat']),
                        int(row['description_length']),
                        row['priority'],
                        int(row['priority_cat'])
                    )
                    
                    cursor.execute(insert_sql, row_data)
                    rows_affected = cursor.rowcount
                    
                    if rows_affected > 0:
                        records_imported += 1
                    # Always count as processed regardless of whether inserted or skipped
                    
                    # Commit every 50 records and refresh connection every 500
                    if (records_processed + 1) % 50 == 0:
                        try:
                            conn.commit()
                            if (records_processed + 1) % 500 == 0:
                                # Refresh connection every 500 records
                                cursor.close()
                                conn.close()
                                print(f"🔄 Refreshing connection at record {records_processed + 1}")
                                conn = get_connection()
                                cursor = conn.cursor()
                        except Exception as conn_error:
                            print(f"⚠️  Connection issue at record {records_processed + 1}: {conn_error}")
                            # Try to reconnect
                            try:
                                cursor.close()
                                conn.close()
                            except Exception:
                                pass
                            conn = get_connection()
                            cursor = conn.cursor()
                        
                except Exception as e:
                    print(f"❌ Error processing row {current_row}: {e}")
                    print(f"Row sample: {dict(list(row.items())[:3])}")
                    # Skip problematic rows but continue
                    pass
                
                records_processed += 1
                current_row += 1
            
            # Final commit
            try:
                conn.commit()
            except Exception as final_commit_error:
                print(f"⚠️  Final commit error: {final_commit_error}")
            
            print(f"✅ Chunk complete: {records_imported:,} new records imported, {records_processed - records_imported:,} duplicates skipped")
            
    except Exception as e:
        print(f"❌ Error in chunk: {e}")
        return False, 0, False
    finally:
        # Clean up connections
        try:
            if cursor:
                cursor.close()
            if conn:
                conn.close()
        except Exception:
            pass
            
    return True, records_imported, False  # False = not end of file


def main():
    """Main function with chunked import"""
    # Get the directory where this script is located
    script_dir = os.path.dirname(os.path.abspath(__file__))
    csv_file_path = os.path.join(script_dir, "data", "Support_tickets.csv")
    
    print("🚀 Robust CSV Import - Processing in chunks")
    print("=" * 50)
    print(f"📁 Looking for CSV file at: {csv_file_path}")
    
    # Import in smaller chunks to avoid connection timeouts
    chunk_size = 1000  # Reduced from 5000 to 1000
    start_row = 1
    total_imported = 0
    
    while True:
        print(f"\n📦 Processing chunk starting at row {start_row:,}")
        
        success, imported, end_of_file = import_csv_chunk(
            csv_file_path, start_row, chunk_size)
        
        if not success:
            print(f"❌ Failed at row {start_row}")
            break
            
        total_imported += imported
        
        if end_of_file:
            print("✅ All data processed!")
            break
            
        start_row += chunk_size
        
        # Brief pause between chunks
        print("⏳ Pausing between chunks...")
        time.sleep(2)
    
    print("\n🎉 Import Summary:")
    print(f"   ✅ Total records imported: {total_imported:,}")


if __name__ == "__main__":
    main()
