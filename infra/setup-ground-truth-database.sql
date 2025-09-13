-- Master setup script for Ground Truth Curation database
-- This script provides the correct order to run the table creation scripts
-- Run these scripts in order after creating the database

/*
SETUP INSTRUCTIONS:
Execute the following SQL scripts in this exact order:

1. First, run: create-ground-truth-tables.sql
   - Creates all main tables with proper foreign key relationships

2. Second, run: create-ground-truth-tag-relationships.sql  
   - Creates the tag relationship table and inserts default tags

3. Optional, run: insert-sample-ground-truth-data.sql
   - Inserts sample data for testing and development
   - SKIP THIS STEP in production environments
*/

PRINT 'Ground Truth Curation database setup complete!';

-- Verify table creation
SELECT 
    TABLE_SCHEMA,
    TABLE_NAME,
    TABLE_TYPE
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN (
    'GROUND_TRUTH_DEFINITION',
    'GROUND_TRUTH_ENTRY', 
    'DATA_QUERY_DEFINITION',
    'CONTEXT',
    'CONTEXT_PARAMETER',
    'COMMENT',
    'TAG',
    'CONVERSATION',
    'GROUND_TRUTH_DEFINITION_CONVERSATION',
    'GROUND_TRUTH_TAG'
)
ORDER BY TABLE_NAME;
