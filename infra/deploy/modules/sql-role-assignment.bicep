@description('SQL Server resource ID')
param sqlServerResourceId string

@description('SQL Database name')
param databaseName string

@description('Principal ID to grant access to')
param principalId string

@description('Display name of the principal (App Service name)')
param principalName string

// Extract server name from resource ID
var serverName = last(split(sqlServerResourceId, '/'))

// Note: SQL role assignments for Managed Identities must be done via T-SQL
// This module serves as documentation of the required configuration
// Run the following commands as an Azure AD admin on the database:
//
// CREATE USER [${principalName}] FROM EXTERNAL PROVIDER;
// ALTER ROLE db_datareader ADD MEMBER [${principalName}];
// ALTER ROLE db_datawriter ADD MEMBER [${principalName}];
//
// Alternatively, use a post-deployment script or Azure DevOps pipeline task

output sqlServerName string = serverName
output databaseName string = databaseName
output principalId string = principalId
output principalName string = principalName
output instructions string = 'Run SQL: CREATE USER [${principalName}] FROM EXTERNAL PROVIDER; ALTER ROLE db_datareader ADD MEMBER [${principalName}]; ALTER ROLE db_datawriter ADD MEMBER [${principalName}];'
