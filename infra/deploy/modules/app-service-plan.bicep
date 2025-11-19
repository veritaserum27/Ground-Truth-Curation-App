@description('The location where the App Service Plan will be deployed')
param location string = resourceGroup().location

@description('Prefix applied to resource names')
param resourceNamePrefix string

@description('Name of the App Service Plan')
param appServicePlanName string = '${resourceNamePrefix}-asp'

@description('SKU for the App Service Plan')
@allowed([
  'B1'
  'B2'
  'B3'
  'S1'
  'S2'
  'S3'
  'P1v3'
  'P2v3'
  'P3v3'
])
param skuName string = 'B1'

@description('Enable zone redundancy (requires Premium v3 tier)')
param zoneRedundant bool = false

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: skuName
  }
  kind: 'linux'
  properties: {
    reserved: true // Required for Linux
    zoneRedundant: zoneRedundant
  }
}

output appServicePlanId string = appServicePlan.id
output appServicePlanName string = appServicePlan.name
