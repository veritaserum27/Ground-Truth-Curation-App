-- Create ground truth curation tables based on the ERD
-- This script should be run after the database is created
-- Tables are created in dependency order to respect foreign key constraints

-- 1. Create GROUND_TRUTH_DEFINITION table (base entity)
CREATE TABLE GROUND_TRUTH_DEFINITION (
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

-- 2. Create GROUND_TRUTH_ENTRY table
CREATE TABLE GROUND_TRUTH_ENTRY (
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

-- 3. Create DATA_QUERY_DEFINITION table
CREATE TABLE DATA_QUERY_DEFINITION (
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

-- 4. Create CONTEXT table
CREATE TABLE CONTEXT (
    contextId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    groundTruthId UNIQUEIDENTIFIER,
    groundTruthEntryId UNIQUEIDENTIFIER,
    contextType NVARCHAR(100) NOT NULL,
    FOREIGN KEY (groundTruthId) REFERENCES GROUND_TRUTH_DEFINITION(groundTruthId),
    FOREIGN KEY (groundTruthEntryId) REFERENCES GROUND_TRUTH_ENTRY(groundTruthEntryId)
);

-- 5. Create CONTEXT_PARAMETER table
CREATE TABLE CONTEXT_PARAMETER (
    parameterId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    contextId UNIQUEIDENTIFIER NOT NULL,
    parameterName NVARCHAR(100) NOT NULL,
    parameterValue NVARCHAR(MAX) NOT NULL,
    dataType NVARCHAR(50) NOT NULL,
    FOREIGN KEY (contextId) REFERENCES CONTEXT(contextId)
);

-- 6. Create COMMENT table
CREATE TABLE COMMENT (
    commentId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    groundTruthId UNIQUEIDENTIFIER NOT NULL,
    comment NVARCHAR(MAX) NOT NULL,
    commentDateTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    userId NVARCHAR(100) NOT NULL,
    commentType NVARCHAR(50) NOT NULL,
    FOREIGN KEY (groundTruthId) REFERENCES GROUND_TRUTH_DEFINITION(groundTruthId)
);

-- 7. Create TAG table
CREATE TABLE TAG (
    tagId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    name NVARCHAR(100) NOT NULL UNIQUE
);

-- 8. Create CONVERSATION table
CREATE TABLE CONVERSATION (
    conversationId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    contextId UNIQUEIDENTIFIER NOT NULL,
    FOREIGN KEY (contextId) REFERENCES CONTEXT(contextId)
);

-- 9. Create GROUND_TRUTH_DEFINITION_CONVERSATION relationship table
CREATE TABLE GROUND_TRUTH_DEFINITION_CONVERSATION (
    conversationId UNIQUEIDENTIFIER NOT NULL,
    groundTruthId UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY (conversationId, groundTruthId),
    FOREIGN KEY (conversationId) REFERENCES CONVERSATION(conversationId),
    FOREIGN KEY (groundTruthId) REFERENCES GROUND_TRUTH_DEFINITION(groundTruthId)
);

-- Create indexes for commonly queried columns
CREATE INDEX IX_ground_truth_definition_validation_status ON GROUND_TRUTH_DEFINITION(validationStatus);
CREATE INDEX IX_ground_truth_definition_user_created ON GROUND_TRUTH_DEFINITION(userCreated);
CREATE INDEX IX_ground_truth_definition_creation_datetime ON GROUND_TRUTH_DEFINITION(creationDateTime);
CREATE INDEX IX_ground_truth_definition_start_datetime ON GROUND_TRUTH_DEFINITION(startDateTime);

CREATE INDEX IX_ground_truth_entry_ground_truth_id ON GROUND_TRUTH_ENTRY(groundTruthId);
CREATE INDEX IX_ground_truth_entry_creation_datetime ON GROUND_TRUTH_ENTRY(creationDateTime);

CREATE INDEX IX_data_query_definition_ground_truth_id ON DATA_QUERY_DEFINITION(groundTruthId);
CREATE INDEX IX_data_query_definition_datastore_type ON DATA_QUERY_DEFINITION(datastoreType);
CREATE INDEX IX_data_query_definition_datastore_name ON DATA_QUERY_DEFINITION(datastoreName);
CREATE INDEX IX_data_query_definition_query_target ON DATA_QUERY_DEFINITION(queryTarget);

CREATE INDEX IX_context_ground_truth_id ON CONTEXT(groundTruthId);
CREATE INDEX IX_context_ground_truth_entry_id ON CONTEXT(groundTruthEntryId);
CREATE INDEX IX_context_context_type ON CONTEXT(contextType);

CREATE INDEX IX_context_parameter_context_id ON CONTEXT_PARAMETER(contextId);
CREATE INDEX IX_context_parameter_parameter_name ON CONTEXT_PARAMETER(parameterName);

CREATE INDEX IX_comment_ground_truth_id ON COMMENT(groundTruthId);
CREATE INDEX IX_comment_user_id ON COMMENT(userId);
CREATE INDEX IX_comment_comment_type ON COMMENT(commentType);
CREATE INDEX IX_comment_comment_datetime ON COMMENT(commentDateTime);

CREATE INDEX IX_tag_name ON TAG(name);

CREATE INDEX IX_conversation_context_id ON CONVERSATION(contextId);

CREATE INDEX IX_gt_def_conv_conversation_id ON GROUND_TRUTH_DEFINITION_CONVERSATION(conversationId);
CREATE INDEX IX_gt_def_conv_ground_truth_id ON GROUND_TRUTH_DEFINITION_CONVERSATION(groundTruthId);
