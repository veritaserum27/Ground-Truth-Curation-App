-- Creates tag metadata and relationships for the Ground Truth curation schema
-- Run this script after create-ground-truth-tables.sql so base tables exist

-- Create TAG table if it does not exist
IF NOT EXISTS (
    SELECT *
FROM sys.tables
WHERE name = 'TAG'
)
BEGIN
  CREATE TABLE TAG
  (
    tagId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    name NVARCHAR(100) NOT NULL UNIQUE,
    description NVARCHAR(MAX) NOT NULL,
    creationDateTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    userCreated NVARCHAR(100) NULL,
    userUpdated NVARCHAR(100) NULL
  );
END

-- Create GROUND_TRUTH_TAG table if it does not exist
IF NOT EXISTS (
    SELECT *
FROM sys.tables
WHERE name = 'GROUND_TRUTH_TAG'
)
BEGIN
  CREATE TABLE GROUND_TRUTH_TAG
  (
    groundTruthId UNIQUEIDENTIFIER NOT NULL,
    tagId UNIQUEIDENTIFIER NOT NULL,
    createdBy NVARCHAR(100) NOT NULL,
    creationDateTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    PRIMARY KEY (groundTruthId, tagId),
    FOREIGN KEY (groundTruthId) REFERENCES GROUND_TRUTH_DEFINITION(groundTruthId),
    FOREIGN KEY (tagId) REFERENCES TAG(tagId)
  );
END

-- Ensure indexes exist for fast filtering
IF NOT EXISTS (
    SELECT *
FROM sys.indexes
WHERE name = 'IX_tag_name'
  AND object_id = OBJECT_ID('TAG')
)
    CREATE INDEX IX_tag_name ON TAG(name);

IF NOT EXISTS (
    SELECT *
FROM sys.indexes
WHERE name = 'IX_ground_truth_tag_groundtruthid'
  AND object_id = OBJECT_ID('GROUND_TRUTH_TAG')
)
    CREATE INDEX IX_ground_truth_tag_groundtruthid ON GROUND_TRUTH_TAG(groundTruthId);

IF NOT EXISTS (
    SELECT *
FROM sys.indexes
WHERE name = 'IX_ground_truth_tag_tagid'
  AND object_id = OBJECT_ID('GROUND_TRUTH_TAG')
)
    CREATE INDEX IX_ground_truth_tag_tagid ON GROUND_TRUTH_TAG(tagId);

-- Seed default tags if they do not already exist
DECLARE @defaultTags TABLE (name NVARCHAR(100),
  description NVARCHAR(MAX));
INSERT INTO @defaultTags
  (name, description)
VALUES
  ('Draft', 'Indicates the ground truth is in draft status'),
  ('Validated', 'Indicates the ground truth has been validated'),
  ('Answerable', 'Indicates the ground truth is answerable'),
  ('Universal', 'Indicates the ground truth is universally applicable'),
  ('Unanswerable', 'Indicates the ground truth is unanswerable'),
  ('Product Defect', 'Related to product defects'),
  ('Customer Issue', 'Related to customer issues'),
  ('Performance', 'Related to performance metrics'),
  ('High Priority', 'Indicates high priority items');

MERGE TAG AS target
USING @defaultTags AS source
    ON target.name = source.name
WHEN NOT MATCHED THEN
    INSERT (tagId, name, description)
    VALUES (NEWID(), source.name, source.description);
