-- Master setup script for Ground Truth Curation database
-- This script provides the correct order to run the table creation scripts
-- Run these scripts in order after creating the database

/*
SETUP INSTRUCTIONS:
Execute the following SQL scripts in this exact order:

1. First, run: create-ground-truth-tables.sql
   - Creates all core tables with foreign key relationships

2. Second, run: create-conversation-tables.sql
   - Creates conversation metadata and linking tables

3. Third, run: create-ground-truth-tag-relationships.sql
   - Creates tag metadata and relationship tables; seeds default tags

4. Optional, run: insert-sample-ground-truth-data.sql
   - Inserts sample data for testing and development
   - SKIP THIS STEP in production environments

5. Optional (development only): drop-ground-truth-tables.sql
   - Removes all Ground Truth tables to allow for a clean re-run
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
    'GROUND_TRUTH_CONTEXT',
    'CONTEXT_PARAMETER',
    'COMMENT',
    'TAG',
    'CONVERSATION',
    'GROUND_TRUTH_DEFINITION_CONVERSATION',
    'GROUND_TRUTH_TAG'
)
ORDER BY TABLE_NAME;
