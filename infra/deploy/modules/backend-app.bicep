@description('The location where the backend app will be deployed')
param location string = resourceGroup().location

@description('Prefix applied to resource names')
param resourceNamePrefix string

@description('Name of the backend App Service')
param appName string = '${resourceNamePrefix}-backend'

@description('Resource ID of the App Service Plan')
param appServicePlanId string

@description('Resource ID of the subnet for VNet integration')
param dataSubnetId string

@description('Frontend URL for CORS configuration')
param frontendUrl string

@description('Ground Truth SQL Server FQDN')
param groundTruthSqlServerFqdn string

@description('Ground Truth Database name')
param groundTruthDatabaseName string

@description('System SQL Server FQDN')
param systemSqlServerFqdn string

@description('System Database name')
param systemDatabaseName string

@description('Cosmos DB account endpoint')
param cosmosDbAccountEndpoint string

@description('Cosmos DB database name')
param cosmosDbDatabaseName string

resource backendApp 'Microsoft.Web/sites@2023-12-01' = {
  name: appName
  location: location
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      alwaysOn: true
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      http20Enabled: true
      cors: {
        allowedOrigins: [
          frontendUrl
          'https://${appName}.azurewebsites.net'
        ]
        supportCredentials: true
      }
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'ASPNETCORE_URLS'
          value: 'http://+:8080'
        }
        {
          name: 'Datastores__GroundTruthDB__ConnectionString'
          value: 'Server=tcp:${groundTruthSqlServerFqdn},1433;Initial Catalog=${groundTruthDatabaseName};Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
        }
        {
          name: 'Datastores__ManufacturingDataRelDB__ConnectionString'
          value: 'Server=tcp:${systemSqlServerFqdn},1433;Initial Catalog=${systemDatabaseName};Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
        }
        {
          name: 'Datastores__ManufacturingDataDocDB__ConnectionString'
          value: 'AccountEndpoint=${cosmosDbAccountEndpoint};'
        }
        {
          name: 'Datastores__ManufacturingDataDocDB__DatabaseName'
          value: cosmosDbDatabaseName
        }
      ]
      healthCheckPath: '/healthz'
    }
    vnetRouteAllEnabled: true
    virtualNetworkSubnetId: dataSubnetId
  }
}

output appName string = backendApp.name
output appUrl string = 'https://${backendApp.properties.defaultHostName}'
output principalId string = backendApp.identity.principalId
output defaultHostName string = backendApp.properties.defaultHostName
