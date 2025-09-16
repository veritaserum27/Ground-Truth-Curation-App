-- Create ground truth curation tables based on the ERD
-- This script should be run after the database is created
-- Tables are created in dependency order to respect foreign key constraints

-- 1. Create GROUND_TRUTH_DEFINITION table (base entity)
IF NOT EXISTS (SELECT *
FROM sys.tables
WHERE name = 'GROUND_TRUTH_DEFINITION')
BEGIN
    CREATE TABLE GROUND_TRUTH_DEFINITION
    (
        groundTruthId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        userQuery NVARCHAR(MAX) NOT NULL,
        validationStatus NVARCHAR(50) NOT NULL
            CONSTRAINT CK_ValidationStatus CHECK (validationStatus IN (
                'Validated', 'Revisions Requested', 'Revised', 'Pending', 
                'Out of Scope', 'New', 'New, Data Curated'
            )),
        userCreated NVARCHAR(100) NOT NULL,
        userUpdated NVARCHAR(100),
        creationDateTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        startDateTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        endDateTime DATETIME2 NULL
    );
END

-- 2. Create GROUND_TRUTH_ENTRY table
IF NOT EXISTS (SELECT *
FROM sys.tables
WHERE name = 'GROUND_TRUTH_ENTRY')
BEGIN
    CREATE TABLE GROUND_TRUTH_ENTRY
    (
        groundTruthEntryId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        groundTruthId UNIQUEIDENTIFIER NOT NULL,
        response NVARCHAR(MAX) NOT NULL,
        requiredValuesJSON NVARCHAR(MAX),
        rawDataJSON NVARCHAR(MAX),
        creationDateTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        startDateTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        endDateTime DATETIME2 NULL,
        FOREIGN KEY (groundTruthId) REFERENCES GROUND_TRUTH_DEFINITION(groundTruthId)
    );
END

-- 3. Create DATA_QUERY_DEFINITION table
IF NOT EXISTS (SELECT *
FROM sys.tables
WHERE name = 'DATA_QUERY_DEFINITION')
BEGIN
    CREATE TABLE DATA_QUERY_DEFINITION
    (
        dataQueryId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        groundTruthId UNIQUEIDENTIFIER NOT NULL,
        datastoreType NVARCHAR(50) NOT NULL,
        datastoreName NVARCHAR(100) NOT NULL,
        queryTarget NVARCHAR(100) NOT NULL,
        queryDefinition NVARCHAR(MAX) NOT NULL,
        isFullQuery BIT NOT NULL DEFAULT 0,
        requiredPropertiesJSON NVARCHAR(MAX),
        userCreated NVARCHAR(100) NOT NULL,
        userUpdated NVARCHAR(100),
        creationDateTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        startDateTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        endDateTime DATETIME2 NULL,
        FOREIGN KEY (groundTruthId) REFERENCES GROUND_TRUTH_DEFINITION(groundTruthId)
    );
END

-- 4. Create GROUND_TRUTH_CONTEXT table
IF NOT EXISTS (SELECT *
FROM sys.tables
WHERE name = 'GROUND_TRUTH_CONTEXT')
BEGIN
    CREATE TABLE GROUND_TRUTH_CONTEXT
    (
        contextId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        groundTruthId UNIQUEIDENTIFIER,
        groundTruthEntryId UNIQUEIDENTIFIER,
        contextType NVARCHAR(100) NOT NULL,
        FOREIGN KEY (groundTruthId) REFERENCES GROUND_TRUTH_DEFINITION(groundTruthId),
        FOREIGN KEY (groundTruthEntryId) REFERENCES GROUND_TRUTH_ENTRY(groundTruthEntryId)
    );
END

-- 5. Create CONTEXT_PARAMETER table
IF NOT EXISTS (SELECT *
FROM sys.tables
WHERE name = 'CONTEXT_PARAMETER')
BEGIN
    CREATE TABLE CONTEXT_PARAMETER
    (
        parameterId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        contextId UNIQUEIDENTIFIER NOT NULL,
        parameterName NVARCHAR(100) NOT NULL,
        parameterValue NVARCHAR(MAX) NOT NULL,
        dataType NVARCHAR(50) NOT NULL,
        FOREIGN KEY (contextId) REFERENCES GROUND_TRUTH_CONTEXT(contextId)
    );
END

-- 6. Create COMMENT table
IF NOT EXISTS (SELECT *
FROM sys.tables
WHERE name = 'COMMENT')
BEGIN
    CREATE TABLE COMMENT
    (
        commentId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        groundTruthId UNIQUEIDENTIFIER NOT NULL,
        comment NVARCHAR(MAX) NOT NULL,
        commentDateTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        userId NVARCHAR(100) NOT NULL,
        commentType NVARCHAR(50) NOT NULL,
        FOREIGN KEY (groundTruthId) REFERENCES GROUND_TRUTH_DEFINITION(groundTruthId)
    );
END

-- 7. Create TAG table
IF NOT EXISTS (SELECT *
FROM sys.tables
WHERE name = 'TAG')
BEGIN
    CREATE TABLE TAG
    (
        tagId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        name NVARCHAR(100) NOT NULL UNIQUE,
        description NVARCHAR(MAX) NOT NULL
    );
END

-- 8. Create CONVERSATION table
IF NOT EXISTS (SELECT *
FROM sys.tables
WHERE name = 'CONVERSATION')
BEGIN
    CREATE TABLE CONVERSATION
    (
        conversationId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        contextId UNIQUEIDENTIFIER NOT NULL UNIQUE,
        FOREIGN KEY (contextId) REFERENCES GROUND_TRUTH_CONTEXT(contextId)
    );
END

-- 9. Create GROUND_TRUTH_DEFINITION_CONVERSATION relationship table
IF NOT EXISTS (SELECT *
FROM sys.tables
WHERE name = 'GROUND_TRUTH_DEFINITION_CONVERSATION')
BEGIN
    CREATE TABLE GROUND_TRUTH_DEFINITION_CONVERSATION
    (
        conversationId UNIQUEIDENTIFIER NOT NULL,
        groundTruthId UNIQUEIDENTIFIER NOT NULL,
        PRIMARY KEY (conversationId, groundTruthId),
        FOREIGN KEY (conversationId) REFERENCES CONVERSATION(conversationId),
        FOREIGN KEY (groundTruthId) REFERENCES GROUND_TRUTH_DEFINITION(groundTruthId)
    );
END

-- 10. Create GROUND_TRUTH_DEFINITION_TAG relationship table
IF NOT EXISTS (SELECT *
FROM sys.tables
WHERE name = 'GROUND_TRUTH_TAG')
BEGIN
    CREATE TABLE GROUND_TRUTH_TAG
    (
        groundTruthId UNIQUEIDENTIFIER NOT NULL,
        tagId UNIQUEIDENTIFIER NOT NULL,
        createdBy NVARCHAR(100) NOT NULL,
        PRIMARY KEY (groundTruthId, tagId),
        FOREIGN KEY (groundTruthId) REFERENCES GROUND_TRUTH_DEFINITION(groundTruthId),
        FOREIGN KEY (tagId) REFERENCES TAG(tagId)
    );
END

-- Create indexes for commonly queried columns
IF NOT EXISTS (SELECT *
FROM sys.indexes
WHERE name = 'IX_ground_truth_definition_validation_status')
    CREATE INDEX IX_ground_truth_definition_validation_status ON GROUND_TRUTH_DEFINITION(validationStatus);
IF NOT EXISTS (SELECT *
FROM sys.indexes
WHERE name = 'IX_ground_truth_definition_user_created')
    CREATE INDEX IX_ground_truth_definition_user_created ON GROUND_TRUTH_DEFINITION(userCreated);
IF NOT EXISTS (SELECT *
FROM sys.indexes
WHERE name = 'IX_ground_truth_definition_creation_datetime')
    CREATE INDEX IX_ground_truth_definition_creation_datetime ON GROUND_TRUTH_DEFINITION(creationDateTime);
IF NOT EXISTS (SELECT *
FROM sys.indexes
WHERE name = 'IX_ground_truth_definition_start_datetime')
    CREATE INDEX IX_ground_truth_definition_start_datetime ON GROUND_TRUTH_DEFINITION(startDateTime);

IF NOT EXISTS (SELECT *
FROM sys.indexes
WHERE name = 'IX_ground_truth_entry_ground_truth_id')
    CREATE INDEX IX_ground_truth_entry_ground_truth_id ON GROUND_TRUTH_ENTRY(groundTruthId);
IF NOT EXISTS (SELECT *
FROM sys.indexes
WHERE name = 'IX_ground_truth_entry_creation_datetime')
    CREATE INDEX IX_ground_truth_entry_creation_datetime ON GROUND_TRUTH_ENTRY(creationDateTime);

IF NOT EXISTS (SELECT *
FROM sys.indexes
WHERE name = 'IX_data_query_definition_ground_truth_id')
    CREATE INDEX IX_data_query_definition_ground_truth_id ON DATA_QUERY_DEFINITION(groundTruthId);
IF NOT EXISTS (SELECT *
FROM sys.indexes
WHERE name = 'IX_data_query_definition_datastore_type')
    CREATE INDEX IX_data_query_definition_datastore_type ON DATA_QUERY_DEFINITION(datastoreType);
IF NOT EXISTS (SELECT *
FROM sys.indexes
WHERE name = 'IX_data_query_definition_datastore_name')
    CREATE INDEX IX_data_query_definition_datastore_name ON DATA_QUERY_DEFINITION(datastoreName);
IF NOT EXISTS (SELECT *
FROM sys.indexes
WHERE name = 'IX_data_query_definition_query_target')
    CREATE INDEX IX_data_query_definition_query_target ON DATA_QUERY_DEFINITION(queryTarget);

IF NOT EXISTS (SELECT *
FROM sys.indexes
WHERE name = 'IX_ground_truth_context_ground_truth_id')
    CREATE INDEX IX_ground_truth_context_ground_truth_id ON GROUND_TRUTH_CONTEXT(groundTruthId);
IF NOT EXISTS (SELECT *
FROM sys.indexes
WHERE name = 'IX_ground_truth_context_ground_truth_entry_id')
    CREATE INDEX IX_ground_truth_context_ground_truth_entry_id ON GROUND_TRUTH_CONTEXT(groundTruthEntryId);
IF NOT EXISTS (SELECT *
FROM sys.indexes
WHERE name = 'IX_ground_truth_context_context_type')
    CREATE INDEX IX_ground_truth_context_context_type ON GROUND_TRUTH_CONTEXT(contextType);

IF NOT EXISTS (SELECT *
FROM sys.indexes
WHERE name = 'IX_context_parameter_context_id')
    CREATE INDEX IX_context_parameter_context_id ON CONTEXT_PARAMETER(contextId);
IF NOT EXISTS (SELECT *
FROM sys.indexes
WHERE name = 'IX_context_parameter_parameter_name')
    CREATE INDEX IX_context_parameter_parameter_name ON CONTEXT_PARAMETER(parameterName);

IF NOT EXISTS (SELECT *
FROM sys.indexes
WHERE name = 'IX_comment_ground_truth_id')
    CREATE INDEX IX_comment_ground_truth_id ON COMMENT(groundTruthId);
IF NOT EXISTS (SELECT *
FROM sys.indexes
WHERE name = 'IX_comment_user_id')
    CREATE INDEX IX_comment_user_id ON COMMENT(userId);
IF NOT EXISTS (SELECT *
FROM sys.indexes
WHERE name = 'IX_comment_comment_type')
    CREATE INDEX IX_comment_comment_type ON COMMENT(commentType);
IF NOT EXISTS (SELECT *
FROM sys.indexes
WHERE name = 'IX_comment_comment_datetime')
    CREATE INDEX IX_comment_comment_datetime ON COMMENT(commentDateTime);

IF NOT EXISTS (SELECT *
FROM sys.indexes
WHERE name = 'IX_tag_name')
    CREATE INDEX IX_tag_name ON TAG(name);

IF NOT EXISTS (SELECT *
FROM sys.indexes
WHERE name = 'IX_conversation_context_id')
    CREATE INDEX IX_conversation_context_id ON CONVERSATION(contextId);

IF NOT EXISTS (SELECT *
FROM sys.indexes
WHERE name = 'IX_gt_def_conv_conversation_id')
    CREATE INDEX IX_gt_def_conv_conversation_id ON GROUND_TRUTH_DEFINITION_CONVERSATION(conversationId);
IF NOT EXISTS (SELECT *
FROM sys.indexes
WHERE name = 'IX_gt_def_conv_ground_truth_id')
    CREATE INDEX IX_gt_def_conv_ground_truth_id ON GROUND_TRUTH_DEFINITION_CONVERSATION(groundTruthId);
