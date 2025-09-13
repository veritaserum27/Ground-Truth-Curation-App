# Ground Truth Curation Database Scripts

This folder contains SQL scripts to create the database schema for the Ground Truth Curation application based on the Entity Relationship Diagram (ERD) defined in `docs/GroundTruthERD.md`.

## Database Scripts

### Core Table Creation Scripts

1. **`create-ground-truth-tables.sql`**
   - Creates all main tables and their relationships
   - Includes proper foreign key constraints
   - Creates indexes for commonly queried columns
   - Tables created (in dependency order):
     - `GROUND_TRUTH_DEFINITION` (base entity)
     - `GROUND_TRUTH_ENTRY`
     - `DATA_QUERY_DEFINITION`
     - `CONTEXT`
     - `CONTEXT_PARAMETER`
     - `COMMENT`
     - `TAG`
     - `CONVERSATION`
     - `GROUND_TRUTH_DEFINITION_CONVERSATION` (relationship table)

2. **`create-ground-truth-tag-relationships.sql`**
   - Creates the many-to-many relationship table between Ground Truth Definitions and Tags
   - Creates the `GROUND_TRUTH_TAG` junction table
   - Inserts default tags for common use cases

### Utility Scripts

1. **`insert-sample-ground-truth-data.sql`**
   - Provides sample data for testing and development
   - Includes realistic examples of all table types
   - Should NOT be run in production environments

1. **`setup-ground-truth-database.sql`**
   - Master instructions file showing the correct order to run scripts
   - Contains documentation and setup guidance

## Setup Instructions

### For Development/Testing

1. Create your SQL Server database
2. Run `create-ground-truth-tables.sql`
3. Run `create-ground-truth-tag-relationships.sql`
4. (Optional) Run `insert-sample-ground-truth-data.sql` for sample data

### For Production

1. Create your SQL Server database
2. Run `create-ground-truth-tables.sql`
3. Run `create-ground-truth-tag-relationships.sql`
4. ⚠️ **DO NOT** run the sample data script

## Table Relationships

The database implements the following key relationships:

- **One-to-Many**: `GROUND_TRUTH_DEFINITION` → `GROUND_TRUTH_ENTRY`
- **One-to-Many**: `GROUND_TRUTH_DEFINITION` → `DATA_QUERY_DEFINITION`
- **One-to-Many**: `GROUND_TRUTH_DEFINITION` → `COMMENT`
- **Many-to-Many**: `GROUND_TRUTH_DEFINITION` ↔ `TAG` (via `GROUND_TRUTH_TAG`)
- **Many-to-Many**: `GROUND_TRUTH_DEFINITION` ↔ `CONVERSATION` (via `GROUND_TRUTH_DEFINITION_CONVERSATION`)
- **One-to-Many**: `CONTEXT` → `CONTEXT_PARAMETER`
- **One-to-One**: `CONVERSATION` → `CONTEXT`

## Key Features

### Temporal Tracking
Tables include `startDateTime` and `endDateTime` columns to track record versions over time. When `endDateTime` is NULL, the record is considered current/active.

### Audit Trail
Most tables include:
- `creationDateTime`: When the record was created
- `userCreated`: User who created the record
- `userUpdated`: User who last updated the record

### Flexible Data Storage
- JSON columns (`requiredValuesJSON`, `rawDataJSON`, `requiredPropertiesJSON`) allow flexible data structures
- `CONTEXT_PARAMETER` table supports dynamic context variables with type information

### Performance Optimization
- Comprehensive indexing strategy for common query patterns
- Foreign key relationships maintain data integrity
- UNIQUEIDENTIFIER primary keys for distributed scenarios

## Data Types Used

- **`UNIQUEIDENTIFIER`**: Primary keys (with `NEWID()` defaults)
- **`NVARCHAR(MAX)`**: Large text fields (queries, responses, JSON)
- **`NVARCHAR(n)`**: Constrained text fields with appropriate lengths
- **`DATETIME2`**: Timestamps (with `GETUTCDATE()` defaults for creation times)
- **`BIT`**: Boolean flags

## Notes

- All tables use `UNIQUEIDENTIFIER` primary keys for distributed system compatibility
- Unicode support throughout with `NVARCHAR` data types
- Comprehensive indexing strategy for performance
- Foreign key constraints ensure data integrity
- Default values and constraints minimize data entry errors

## References

- **ERD Source**: `docs/GroundTruthERD.md`
- **Process Documentation**: `docs/GroundTruthCurationProcess.md`
- **Architecture**: `docs/Architecture.md`
