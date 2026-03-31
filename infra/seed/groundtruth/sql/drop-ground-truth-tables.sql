-- DEVELOPMENT ONLY: drops Ground Truth curation tables
-- ⚠️ DO NOT run in shared or production environments

-- Disable foreign key checks for the duration of drops
DECLARE @sql NVARCHAR(MAX) = '';
DECLARE @crlf NCHAR(2) = NCHAR(13) + NCHAR(10);

-- Drop relationship tables first
IF OBJECT_ID('GROUND_TRUTH_DEFINITION_CONVERSATION', 'U') IS NOT NULL
    SET @sql += 'DROP TABLE GROUND_TRUTH_DEFINITION_CONVERSATION;' + @crlf;
IF OBJECT_ID('GROUND_TRUTH_TAG', 'U') IS NOT NULL
    SET @sql += 'DROP TABLE GROUND_TRUTH_TAG;' + @crlf;

-- Drop dependent tables
IF OBJECT_ID('CONVERSATION', 'U') IS NOT NULL
    SET @sql += 'DROP TABLE CONVERSATION;' + @crlf;
IF OBJECT_ID('COMMENT', 'U') IS NOT NULL
    SET @sql += 'DROP TABLE COMMENT;' + @crlf;
IF OBJECT_ID('CONTEXT_PARAMETER', 'U') IS NOT NULL
    SET @sql += 'DROP TABLE CONTEXT_PARAMETER;' + @crlf;
IF OBJECT_ID('GROUND_TRUTH_CONTEXT', 'U') IS NOT NULL
    SET @sql += 'DROP TABLE GROUND_TRUTH_CONTEXT;' + @crlf;
IF OBJECT_ID('GROUND_TRUTH_ENTRY', 'U') IS NOT NULL
    SET @sql += 'DROP TABLE GROUND_TRUTH_ENTRY;' + @crlf;
IF OBJECT_ID('DATA_QUERY_DEFINITION', 'U') IS NOT NULL
    SET @sql += 'DROP TABLE DATA_QUERY_DEFINITION;' + @crlf;
IF OBJECT_ID('TAG', 'U') IS NOT NULL
    SET @sql += 'DROP TABLE TAG;' + @crlf;
IF OBJECT_ID('GROUND_TRUTH_DEFINITION', 'U') IS NOT NULL
    SET @sql += 'DROP TABLE GROUND_TRUTH_DEFINITION;' + @crlf;

IF LEN(@sql) = 0
BEGIN
  PRINT 'No Ground Truth curation tables found to drop.';
END
ELSE
BEGIN
  EXEC sp_executesql @sql;
  PRINT 'Ground Truth curation tables dropped (development cleanup).';
END;
