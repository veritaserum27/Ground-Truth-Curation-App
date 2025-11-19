-- Configure SQL Database Access for Backend App Service Managed Identity
-- This script grants the backend App Service Managed Identity access to read/write data
-- Usage: Replace {{BACKEND_APP_NAME}} with the actual backend app service name

-- Check if user already exists, if not create it
IF NOT EXISTS (SELECT *
FROM sys.database_principals
WHERE name = N'{{BACKEND_APP_NAME}}')
BEGIN
  CREATE USER [{{BACKEND_APP_NAME}}] FROM EXTERNAL PROVIDER;
  PRINT 'Created user [{{BACKEND_APP_NAME}}]';
END
ELSE
BEGIN
  PRINT 'User [{{BACKEND_APP_NAME}}] already exists';
END;

-- Grant read access
IF IS_ROLEMEMBER('db_datareader', '{{BACKEND_APP_NAME}}') = 0
BEGIN
  ALTER ROLE db_datareader ADD MEMBER [{{BACKEND_APP_NAME}}];
  PRINT 'Granted db_datareader role to [{{BACKEND_APP_NAME}}]';
END
ELSE
BEGIN
  PRINT '[{{BACKEND_APP_NAME}}] already has db_datareader role';
END;

-- Grant write access
IF IS_ROLEMEMBER('db_datawriter', '{{BACKEND_APP_NAME}}') = 0
BEGIN
  ALTER ROLE db_datawriter ADD MEMBER [{{BACKEND_APP_NAME}}];
  PRINT 'Granted db_datawriter role to [{{BACKEND_APP_NAME}}]';
END
ELSE
BEGIN
  PRINT '[{{BACKEND_APP_NAME}}] already has db_datawriter role';
END;

PRINT 'Database access configuration completed for [{{BACKEND_APP_NAME}}]';
