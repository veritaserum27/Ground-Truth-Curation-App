@description('Cosmos DB account name')
param cosmosDbAccountName string

@description('Principal ID to grant access to')
param principalId string

@description('Role definition ID for Cosmos DB (default: Cosmos DB Built-in Data Contributor)')
param roleDefinitionId string = '00000000-0000-0000-0000-000000000002'

// Reference to existing Cosmos DB account
resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2024-05-15' existing = {
  name: cosmosDbAccountName
}

// Assign Cosmos DB Data Contributor role to the managed identity
resource cosmosRoleAssignment 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2024-05-15' = {
  parent: cosmosAccount
  name: guid(cosmosAccount.id, principalId, roleDefinitionId)
  properties: {
    roleDefinitionId: '${cosmosAccount.id}/sqlRoleDefinitions/${roleDefinitionId}'
    principalId: principalId
    scope: cosmosAccount.id
  }
}

output roleAssignmentId string = cosmosRoleAssignment.id
output principalId string = principalId
