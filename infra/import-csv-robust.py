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

# Database connection parameters
CONNECTION_PARAMS = {
    'server': 'ground-truth-sql-un7clpddqxgbc.database.windows.net',
    'database': 'SystemDemoDB',
    'username': 'gtadmin',
    'password': 'H@nS0l01985',
    'driver': '{ODBC Driver 18 for SQL Server}',
    'port': 1433
}


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
    """Get a fresh database connection"""
    conn_str = create_connection_string(CONNECTION_PARAMS)
    return pyodbc.connect(conn_str)


def import_csv_chunk(csv_file_path: str, start_row: int = 1,
                     chunk_size: int = 5000):
    """Import a chunk of CSV data"""
    
    if not os.path.exists(csv_file_path):
        print(f"❌ Error: CSV file not found at {csv_file_path}")
        return False, 0
    
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
        
        # Prepare INSERT statement
        insert_sql = """
        INSERT INTO support_tickets
        (ticket_id, day_of_week, day_of_week_num, company_id, company_size,
         company_size_cat, industry, industry_cat, customer_tier,
         customer_tier_cat, org_users, region, region_cat, past_30d_tickets,
         past_90d_incidents, product_area, product_area_cat, booking_channel,
         booking_channel_cat, reported_by_role, reported_by_role_cat,
         customers_affected, error_rate_pct, downtime_min, payment_impact_flag,
         security_incident_flag, data_loss_flag, has_runbook,
         customer_sentiment, customer_sentiment_cat, description_length,
         priority, priority_cat)
        VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?,
                ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
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
                    return True, 0
            
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
                    records_imported += 1
                    
                    # Commit every 100 records to avoid connection timeout
                    if records_imported % 100 == 0:
                        conn.commit()
                        
                except Exception as e:
                    print(f"❌ Error processing row {current_row}: {e}")
                    print(f"Row sample: {dict(list(row.items())[:3])}")
                    # Skip problematic rows
                    pass
                
                records_processed += 1
                current_row += 1
            
            # Final commit
            conn.commit()
            
            print(f"✅ Chunk complete: {records_imported:,} records imported")
            
        cursor.close()
        conn.close()
        return True, records_imported
        
    except Exception as e:
        print(f"❌ Error in chunk: {e}")
        return False, 0


def main():
    """Main function with chunked import"""
    csv_file_path = "data/Support_tickets.csv"
    
    print("🚀 Robust CSV Import - Processing in chunks")
    print("=" * 50)
    
    # Import in chunks of 5000 records
    chunk_size = 5000
    start_row = 1
    total_imported = 0
    
    while True:
        print(f"\n📦 Processing chunk starting at row {start_row:,}")
        
        success, imported = import_csv_chunk(
            csv_file_path, start_row, chunk_size)
        
        if not success:
            print(f"❌ Failed at row {start_row}")
            break
            
        total_imported += imported
        
        if imported == 0:
            print("✅ All data imported!")
            break
            
        start_row += chunk_size
        
        # Brief pause between chunks
        print("⏳ Pausing between chunks...")
        time.sleep(2)
    
    print("\n🎉 Import Summary:")
    print(f"   ✅ Total records imported: {total_imported:,}")


if __name__ == "__main__":
    main()
