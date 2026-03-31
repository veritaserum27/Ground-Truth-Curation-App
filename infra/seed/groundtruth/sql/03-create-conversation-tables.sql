-- Creates conversation metadata tables used to associate conversations with ground truths
-- Run this script after create-ground-truth-tables.sql so referenced tables exist

-- Create CONVERSATION table if it does not exist
IF NOT EXISTS (
    SELECT *
FROM sys.tables
WHERE name = 'CONVERSATION'
)
BEGIN
  CREATE TABLE CONVERSATION
  (
    conversationId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    contextId UNIQUEIDENTIFIER NOT NULL,
    transcript NVARCHAR(MAX) NULL,
    summary NVARCHAR(MAX) NULL,
    creationDateTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    userCreated NVARCHAR(100) NULL,
    userUpdated NVARCHAR(100) NULL,
    startDateTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    endDateTime DATETIME2 NULL,
    FOREIGN KEY (contextId) REFERENCES GROUND_TRUTH_CONTEXT(contextId)
  );
END

-- Create relationship table linking conversations to ground truth definitions
IF NOT EXISTS (
    SELECT *
FROM sys.tables
WHERE name = 'GROUND_TRUTH_DEFINITION_CONVERSATION'
)
BEGIN
  CREATE TABLE GROUND_TRUTH_DEFINITION_CONVERSATION
  (
    groundTruthId UNIQUEIDENTIFIER NOT NULL,
    conversationId UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY (groundTruthId, conversationId),
    FOREIGN KEY (groundTruthId) REFERENCES GROUND_TRUTH_DEFINITION(groundTruthId),
    FOREIGN KEY (conversationId) REFERENCES CONVERSATION(conversationId)
  );
END

-- Helpful indexes for common relationship lookups
IF NOT EXISTS (
    SELECT *
FROM sys.indexes
WHERE name = 'IX_conversation_contextId'
  AND object_id = OBJECT_ID('CONVERSATION')
)
    CREATE INDEX IX_conversation_contextId ON CONVERSATION(contextId);

IF NOT EXISTS (
    SELECT *
FROM sys.indexes
WHERE name = 'IX_gt_definition_conversation_groundTruthId'
  AND object_id = OBJECT_ID('GROUND_TRUTH_DEFINITION_CONVERSATION')
)
    CREATE INDEX IX_gt_definition_conversation_groundTruthId
        ON GROUND_TRUTH_DEFINITION_CONVERSATION(groundTruthId);

IF NOT EXISTS (
    SELECT *
FROM sys.indexes
WHERE name = 'IX_gt_definition_conversation_conversationId'
  AND object_id = OBJECT_ID('GROUND_TRUTH_DEFINITION_CONVERSATION')
)
    CREATE INDEX IX_gt_definition_conversation_conversationId
        ON GROUND_TRUTH_DEFINITION_CONVERSATION(conversationId);
