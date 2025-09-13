-- Create ground truth tag relationships and additional tables
-- This script creates any missing relationship tables and utility tables

-- Create junction table for GROUND_TRUTH_DEFINITION and TAG many-to-many relationship
-- This table is implied by the ERD relationship but not explicitly defined
CREATE TABLE GROUND_TRUTH_TAG (
    groundTruthId UNIQUEIDENTIFIER NOT NULL,
    tagId UNIQUEIDENTIFIER NOT NULL,
    createdDateTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    createdBy NVARCHAR(100) NOT NULL,
    PRIMARY KEY (groundTruthId, tagId),
    FOREIGN KEY (groundTruthId) REFERENCES GROUND_TRUTH_DEFINITION(groundTruthId),
    FOREIGN KEY (tagId) REFERENCES TAG(tagId)
);

-- Create indexes for the junction table
CREATE INDEX IX_ground_truth_tag_ground_truth_id ON GROUND_TRUTH_TAG(groundTruthId);
CREATE INDEX IX_ground_truth_tag_tag_id ON GROUND_TRUTH_TAG(tagId);
CREATE INDEX IX_ground_truth_tag_created_datetime ON GROUND_TRUTH_TAG(createdDateTime);

-- Insert some default tags that might be commonly used
INSERT INTO TAG (name) VALUES 
    ('Validated'),
    ('Answerable'),
    ('Unanswerable'),
    ('Universal'),
    ('Draft'),
    ('High Priority'),
    ('Customer Issue'),
    ('Product Defect'),
    ('Data Quality'),
    ('Performance'),
    ('Security'),
    ('Compliance');
