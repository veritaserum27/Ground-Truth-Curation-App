@description('The location where the frontend app will be deployed')
param location string = resourceGroup().location

@description('Prefix applied to resource names')
param resourceNamePrefix string

@description('Name of the frontend App Service')
param appName string = '${resourceNamePrefix}-frontend'

@description('Resource ID of the App Service Plan')
param appServicePlanId string

@description('Resource ID of the subnet for VNet integration')
param dataSubnetId string

@description('URL of the backend API (used for VITE_API_BASE_URL)')
param backendApiUrl string

@description('Node.js version for the runtime')
param nodeVersion string = '20-lts'

resource frontendApp 'Microsoft.Web/sites@2023-12-01' = {
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
      linuxFxVersion: 'NODE|${nodeVersion}'
      alwaysOn: true
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      http20Enabled: true
      appSettings: [
        {
          name: 'VITE_API_BASE_URL'
          value: backendApiUrl
        }
        {
          name: 'SCM_DO_BUILD_DURING_DEPLOYMENT'
          value: 'false'
        }
        {
          name: 'WEBSITE_NODE_DEFAULT_VERSION'
          value: '~20'
        }
      ]
      healthCheckPath: '/'
    }
    vnetRouteAllEnabled: true
    virtualNetworkSubnetId: dataSubnetId
  }
}

output appName string = frontendApp.name
output appUrl string = 'https://${frontendApp.properties.defaultHostName}'
output principalId string = frontendApp.identity.principalId
output defaultHostName string = frontendApp.properties.defaultHostName
