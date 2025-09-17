--Declare variables to store the generated GUIDs
DECLARE @groundTruthId1 UNIQUEIDENTIFIER = NEWID();
DECLARE @groundTruthId2 UNIQUEIDENTIFIER = NEWID();
DECLARE @groundTruthId3 UNIQUEIDENTIFIER = NEWID();

DECLARE @dataQueryId1 UNIQUEIDENTIFIER = NEWID();
DECLARE @dataQueryId2 UNIQUEIDENTIFIER = NEWID();
DECLARE @dataQueryId3 UNIQUEIDENTIFIER = NEWID();
DECLARE @dataQueryId4 UNIQUEIDENTIFIER = NEWID();

DECLARE @groundTruthEntryId1 UNIQUEIDENTIFIER = NEWID();
DECLARE @groundTruthEntryId2 UNIQUEIDENTIFIER = NEWID();
DECLARE @groundTruthEntryId3 UNIQUEIDENTIFIER = NEWID();

DECLARE @contextId1 UNIQUEIDENTIFIER = NEWID();
DECLARE @contextId2 UNIQUEIDENTIFIER = NEWID();
DECLARE @contextId3 UNIQUEIDENTIFIER = NEWID();

DECLARE @parameterId1 UNIQUEIDENTIFIER = NEWID();
DECLARE @parameterId2 UNIQUEIDENTIFIER = NEWID();
DECLARE @parameterId3 UNIQUEIDENTIFIER = NEWID();
DECLARE @parameterId4 UNIQUEIDENTIFIER = NEWID();

DECLARE @commentId1 UNIQUEIDENTIFIER = NEWID();
DECLARE @commentId2 UNIQUEIDENTIFIER = NEWID();

-- Sample Ground Truth Definitions
INSERT INTO GROUND_TRUTH_DEFINITION
    (groundTruthId, userQuery, validationStatus, category, userCreated, userUpdated)
VALUES
    (@groundTruthId1, 'What are the top 5 most common product defects in manufacturing?', 'New, Data Curated', 'Manufacturing', 'user1@company.com', 'user1@company.com'),
    (@groundTruthId2, 'How many support tickets were created last month by priority level?', 'Validated', 'Support', 'user2@company.com', 'user2@company.com'),
    (@groundTruthId3, 'What is the average resolution time for high-priority customer issues?', 'Pending', 'Support', 'user1@company.com', 'user3@company.com');

-- Sample Data Query Definitions
INSERT INTO DATA_QUERY_DEFINITION
    (dataQueryId, groundTruthId, datastoreType, datastoreName, queryTarget, queryDefinition, isFullQuery, requiredPropertiesJSON, userCreated)
VALUES
    (@dataQueryId1, @groundTruthId1, 'SQL', 'ManufacturingDataRelDB', 'support_tickets', 'SELECT product_area, COUNT(*) as defect_count FROM support_tickets WHERE (data_loss_flag = 1 OR payment_impact_flag = 1) AND priority = @severity_threshold AND DATEPART(quarter, GETDATE()) = @analysis_quarter GROUP BY product_area ORDER BY defect_count DESC', 1, '["product_area", "defect_count"]', 'user1@company.com'),
    (@dataQueryId2, @groundTruthId2, 'SQL', 'ManufacturingDataRelDB', 'support_tickets', 'SELECT priority, COUNT(*) as ticket_count FROM support_tickets WHERE DATEPART(month, GETDATE()) = DATEPART(month, DATEADD(month, -1, GETDATE())) GROUP BY priority ORDER BY priority_cat', 1, '["priority", "ticket_count"]', 'user2@company.com'),
    (@dataQueryId3, @groundTruthId3, 'CosmosDB', 'ManufacturingDataDocDB', 'repairs', 'SELECT r.repair_type, AVG(r.resolution_time_hours) as avg_resolution_time FROM repairs r WHERE r.priority = ''High'' AND r.status = ''Completed'' AND r.region = @region AND r.created_date >= DateTimeAdd(''day'', -@time_window_days, GetCurrentDateTime()) GROUP BY r.repair_type', 1, '["repair_type", "avg_resolution_time"]', 'user3@company.com'),
    (@dataQueryId4, @groundTruthId1, 'CosmosDB', 'ManufacturingDataDocDB', 'repairs', 'SELECT * from c LIMIT 5', 1, '["defectType"]', 'user4@company.com');

-- Sample Ground Truth Entries
INSERT INTO GROUND_TRUTH_ENTRY
    (groundTruthEntryId, groundTruthId, response, requiredValuesJSON, rawDataJSON)
VALUES
    (@groundTruthEntryId1, @groundTruthId1, 'The top 5 product areas with the most defects (data loss or payment impact) are: 1) Authentication (127 incidents), 2) Payment Processing (89 incidents), 3) Data Management (76 incidents), 4) API Gateway (45 incidents), 5) User Interface (32 incidents)', '["Authentication", "Payment Processing", "Data Management", "API Gateway", "User Interface"]',
        '[{"dataQueryId": "' + CAST(@dataQueryId1 AS NVARCHAR(36)) + '", "rawData": [{"product_area": "Authentication", "defect_count": 127}, {"product_area": "Payment Processing", "defect_count": 89}, {"product_area": "Data Management", "defect_count": 76}, {"product_area": "API Gateway", "defect_count": 45}, {"product_area": "User Interface", "defect_count": 32}]}, {"dataQueryId": "' + CAST(@dataQueryId4 AS NVARCHAR(36)) + '", "rawData": [{"defectType": "Structural", "defect_id": 100004}, {"defectType": "Structural", "defect_id": 100001}, {"defectType": "Structural", "defect_id": 100002}, {"defectType": "Functional", "defect_id": 100003}, {"defectType": "Cosmetic", "defect_id": 100005}]}]'),
    (@groundTruthEntryId2, @groundTruthId2, 'Last month had 1,247 support tickets distributed as follows: High priority (156 tickets), Medium priority (623 tickets), Low priority (468 tickets)', '["High", "Medium", "Low", "156", "623", "468"]',
        '[{"dataQueryId": "' + CAST(@dataQueryId2 AS NVARCHAR(36)) + '", "rawData": [{"priority": "High", "ticket_count": 156}, {"priority": "Medium", "ticket_count": 623}, {"priority": "Low", "ticket_count": 468}]}]'),
    (@groundTruthEntryId3, @groundTruthId3, 'Average resolution time for high-priority repairs by type: Hardware repairs (4.2 hours), Software repairs (2.8 hours), Network repairs (6.1 hours), Security repairs (8.3 hours)', '["Hardware repairs", "Software repairs", "Network repairs", "Security repairs", "4.2", "2.8", "6.1", "8.3"]',
        '[{"dataQueryId": "' + CAST(@dataQueryId3 AS NVARCHAR(36)) + '", "rawData": [{"repair_type": "Hardware", "avg_resolution_time": 4.2}, {"repair_type": "Software", "avg_resolution_time": 2.8}, {"repair_type": "Network", "avg_resolution_time": 6.1}, {"repair_type": "Security", "avg_resolution_time": 8.3}]}]');

-- Sample Context
INSERT INTO GROUND_TRUTH_CONTEXT
    (contextId, groundTruthId, groundTruthEntryId, contextType)
VALUES
    (@contextId1, @groundTruthId1, @groundTruthEntryId1, 'dynamic'),
    (@contextId2, @groundTruthId2, @groundTruthEntryId2, 'dynamic'),
    (@contextId3, @groundTruthId3, @groundTruthEntryId3, 'dynamic');

-- Sample Context Parameters
INSERT INTO CONTEXT_PARAMETER
    (parameterId, contextId, parameterName, parameterValue, dataType)
VALUES
    (@parameterId1, @contextId1, 'analysis_quarter', '3', 'int'),
    (@parameterId2, @contextId1, 'severity_threshold', 'High', 'string'),
    (@parameterId3, @contextId2, 'region', 'North America', 'string'),
    (@parameterId4, @contextId3, 'time_window_days', '30', 'int');

-- Sample Comments
INSERT INTO COMMENT
    (commentId, groundTruthId, comment, userId, commentType)
VALUES
    (@commentId1, @groundTruthId1, 'This query captures the essential defect patterns but we should consider seasonal variations.', 'reviewer1@company.com', 'Review'),
    (@commentId2, @groundTruthId2, 'Data looks accurate and matches our monthly reporting. Ready for validation.', 'curator1@company.com', 'Curator Note');

INSERT INTO TAG
    (tagId, name, description)
VALUES
    (NEWID(), 'Draft', 'Indicates the ground truth is in draft status'),
    (NEWID(), 'Validated', 'Indicates the ground truth has been validated'),
    (NEWID(), 'Answerable', 'Indicates the ground truth is answerable'),
    (NEWID(), 'Universal', 'Indicates the ground truth is universally applicable'),
    (NEWID(), 'Unanswerable', 'Indicates the ground truth is unanswerable'),
    (NEWID(), 'Product Defect', 'Related to product defects'),
    (NEWID(), 'Customer Issue', 'Related to customer issues'),
    (NEWID(), 'Performance', 'Related to performance metrics'),
    (NEWID(), 'High Priority', 'Indicates high priority items');

-- First ground truth entry (manufacturing defects): Draft, Answerable, Product Defect
-- Tag: Draft
INSERT INTO GROUND_TRUTH_TAG
    (groundTruthId, tagId, createdBy)
SELECT g.groundTruthId, t.tagId, 'user1@company.com'
FROM GROUND_TRUTH_DEFINITION g
    CROSS JOIN TAG t
WHERE g.userQuery = 'What are the top 5 most common product defects in manufacturing?'
    AND t.name = 'Draft';

-- Tag: Answerable
INSERT INTO GROUND_TRUTH_TAG
    (groundTruthId, tagId, createdBy)
SELECT g.groundTruthId, t.tagId, 'system'
FROM GROUND_TRUTH_DEFINITION g
    CROSS JOIN TAG t
WHERE g.userQuery = 'What are the top 5 most common product defects in manufacturing?'
    AND t.name = 'Answerable';

-- Tag: Product Defect
INSERT INTO GROUND_TRUTH_TAG
    (groundTruthId, tagId, createdBy)
SELECT g.groundTruthId, t.tagId, 'user1@company.com'
FROM GROUND_TRUTH_DEFINITION g
    CROSS JOIN TAG t
WHERE g.userQuery = 'What are the top 5 most common product defects in manufacturing?'
    AND t.name = 'Product Defect';

-- Second ground truth entry (support tickets)
INSERT INTO GROUND_TRUTH_TAG
    (groundTruthId, tagId, createdBy)
SELECT g.groundTruthId, t.tagId, 'user2@company.com'
FROM GROUND_TRUTH_DEFINITION g
    CROSS JOIN TAG t
WHERE g.userQuery LIKE '%support tickets%'
    AND t.name = 'Customer Issue';

-- Third ground truth entry (resolution time)
INSERT INTO GROUND_TRUTH_TAG
    (groundTruthId, tagId, createdBy)
SELECT g.groundTruthId, t.tagId, 'system'
FROM GROUND_TRUTH_DEFINITION g
    CROSS JOIN TAG t
WHERE g.userQuery LIKE '%resolution time%'
    AND t.name = 'Customer Issue';

-- Display summary of inserted data
    SELECT 'Ground Truth Definitions' as TableName, COUNT(*) as RecordCount
    FROM GROUND_TRUTH_DEFINITION
UNION ALL
    SELECT 'Data Query Definitions', COUNT(*)
    FROM DATA_QUERY_DEFINITION
UNION ALL
    SELECT 'Ground Truth Entries', COUNT(*)
    FROM GROUND_TRUTH_ENTRY
UNION ALL
    SELECT 'Context Records', COUNT(*)
    FROM GROUND_TRUTH_CONTEXT
UNION ALL
    SELECT 'Context Parameters', COUNT(*)
    FROM CONTEXT_PARAMETER
UNION ALL
    SELECT 'Comments', COUNT(*)
    FROM COMMENT
UNION ALL
    SELECT 'Ground Truth Tags', COUNT(*)
    FROM GROUND_TRUTH_TAG;
