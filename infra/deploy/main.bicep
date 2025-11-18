@description('The location where all resources will be deployed')
param location string = resourceGroup().location

@description('Prefix applied to resource names to avoid conflicts across deployments')
@minLength(1)
param resourceNamePrefix string = 'sample-prefix'

@description('The name of the System SQL Server')
param systemSqlServerName string = '${resourceNamePrefix}-gt-system-data-sql'

@description('The name of the Ground Truth SQL Server')
param groundTruthSqlServerName string = '${resourceNamePrefix}-ground-truth-curation-sql'

@description('The name of the System Demo SQL Database')
param systemDatabaseName string = '${resourceNamePrefix}ManufacturingDataRelDB'

@description('The name of the Ground Truth SQL Database')
param groundTruthDatabaseName string = '${resourceNamePrefix}GroundTruthDB'

@description('Login or display name for the Azure AD administrator of the SQL servers')
param sqlAadAdministratorLogin string

@description('Azure AD object ID for the administrator principal')
param sqlAadAdministratorObjectId string

@description('Azure AD principal type for the administrator (User, Group, ServicePrincipal, Application)')
@allowed([
  'Application'
  'Group'
  'ServicePrincipal'
  'User'
])
param sqlAadAdministratorPrincipalType string = 'User'

@description('Azure AD tenant ID for the administrator principal')
param sqlAadAdministratorTenantId string = tenant().tenantId

@description('The SKU for the SQL Database')
param sqlDatabaseSku object = {
  name: 'Basic'
  tier: 'Basic'
  capacity: 5
}

@description('The name of the Azure Cosmos DB account')
param cosmosDbAccountName string = toLower('${resourceNamePrefix}-system-data-cosmos')

@description('Logical Cosmos DB database name used by seed scripts (database resource is not provisioned by this template)')
param cosmosDbDatabaseName string = '${resourceNamePrefix}ManufacturingDefects'

@description('Name of the virtual network hosting data service endpoints')
param virtualNetworkName string = '${resourceNamePrefix}-vnet'

@description('Address prefixes assigned to the virtual network')
param vnetAddressPrefixes array = [
  '10.10.0.0/16'
]

@description('Name of the subnet dedicated to data service private endpoints')
param dataSubnetName string = 'data-services'

@description('Address prefix assigned to the data services subnet')
param dataSubnetPrefix string = '10.10.1.0/24'

@description('Optional client IPv4 address added to SQL firewall (example: 203.0.113.10)')
@minLength(0)
param adminClientIpAddress string = ''

@description('Private DNS zone name used for Cosmos DB private endpoints')
param cosmosPrivateDnsZoneName string = 'privatelink.documents.azure.com'

module sharedNetworking 'modules/shared-networking.bicep' = {
  name: '${resourceNamePrefix}-sharedNetworking'
  params: {
    location: location
    resourceNamePrefix: resourceNamePrefix
    virtualNetworkName: virtualNetworkName
    vnetAddressPrefixes: vnetAddressPrefixes
    dataSubnetName: dataSubnetName
    dataSubnetPrefix: dataSubnetPrefix
    cosmosPrivateDnsZoneName: cosmosPrivateDnsZoneName
  }
}

module systemDataSql 'modules/system-sql.bicep' = {
  name: '${resourceNamePrefix}-systemDataSql'
  params: {
    location: location
    resourceNamePrefix: resourceNamePrefix
    serverName: systemSqlServerName
    databaseName: systemDatabaseName
    sqlDatabaseSku: sqlDatabaseSku
    sqlAadAdministratorLogin: sqlAadAdministratorLogin
    sqlAadAdministratorObjectId: sqlAadAdministratorObjectId
    sqlAadAdministratorPrincipalType: sqlAadAdministratorPrincipalType
    sqlAadAdministratorTenantId: sqlAadAdministratorTenantId
    adminClientIpAddress: adminClientIpAddress
    dataSubnetId: sharedNetworking.outputs.dataSubnetId
    sqlPrivateDnsZoneId: sharedNetworking.outputs.sqlPrivateDnsZoneId
  }
}

module groundTruthDataSql 'modules/groundtruth-sql.bicep' = {
  name: '${resourceNamePrefix}-groundTruthDataSql'
  params: {
    location: location
    resourceNamePrefix: resourceNamePrefix
    serverName: groundTruthSqlServerName
    databaseName: groundTruthDatabaseName
    sqlDatabaseSku: sqlDatabaseSku
    sqlAadAdministratorLogin: sqlAadAdministratorLogin
    sqlAadAdministratorObjectId: sqlAadAdministratorObjectId
    sqlAadAdministratorPrincipalType: sqlAadAdministratorPrincipalType
    sqlAadAdministratorTenantId: sqlAadAdministratorTenantId
    adminClientIpAddress: adminClientIpAddress
    dataSubnetId: sharedNetworking.outputs.dataSubnetId
    sqlPrivateDnsZoneId: sharedNetworking.outputs.sqlPrivateDnsZoneId
  }
}

module manufacturingCosmos 'modules/cosmos-account.bicep' = {
  name: '${resourceNamePrefix}-manufacturingCosmos'
  params: {
    location: location
    resourceNamePrefix: resourceNamePrefix
    cosmosDbAccountName: cosmosDbAccountName
    cosmosDbDatabaseName: cosmosDbDatabaseName
    dataSubnetId: sharedNetworking.outputs.dataSubnetId
    cosmosPrivateDnsZoneId: sharedNetworking.outputs.cosmosPrivateDnsZoneId
  }
}

output systemSqlServerName string = systemDataSql.outputs.serverName
output systemSqlServerFqdn string = systemDataSql.outputs.serverFqdn
output groundTruthSqlServerName string = groundTruthDataSql.outputs.serverName
output groundTruthSqlServerFqdn string = groundTruthDataSql.outputs.serverFqdn
output systemDatabaseName string = systemDataSql.outputs.databaseName
output groundTruthDatabaseName string = groundTruthDataSql.outputs.databaseName
output connectionStringTemplateSystem string = 'Server=tcp:${systemDataSql.outputs.serverFqdn},1433;Initial Catalog=${systemDataSql.outputs.databaseName};Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
output connectionStringTemplateGroundTruth string = 'Server=tcp:${groundTruthDataSql.outputs.serverFqdn},1433;Initial Catalog=${groundTruthDataSql.outputs.databaseName};Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
output dataVirtualNetworkName string = sharedNetworking.outputs.virtualNetworkName
output dataSubnetResourceId string = sharedNetworking.outputs.dataSubnetId
output systemSqlPrivateEndpointId string = systemDataSql.outputs.privateEndpointId
output groundTruthSqlPrivateEndpointId string = groundTruthDataSql.outputs.privateEndpointId
output cosmosPrivateEndpointId string = manufacturingCosmos.outputs.privateEndpointId
output adminClientIpAddress string = adminClientIpAddress
output sqlFirewallRuleApplied bool = systemDataSql.outputs.firewallRuleApplied

// Cosmos DB outputs
output cosmosDbAccountName string = manufacturingCosmos.outputs.accountName
output cosmosDbAccountEndpoint string = manufacturingCosmos.outputs.documentEndpoint
output cosmosDbResourceId string = manufacturingCosmos.outputs.resourceId
output cosmosDbDatabaseName string = manufacturingCosmos.outputs.databaseName
