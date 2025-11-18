@description('Deploys the Cosmos DB account and private endpoint for manufacturing defects data.')
param location string
param resourceNamePrefix string
param cosmosDbAccountName string
param cosmosDbDatabaseName string
param dataSubnetId string
param cosmosPrivateDnsZoneId string

resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2023-09-15' = {
  name: cosmosDbAccountName
  location: location
  kind: 'GlobalDocumentDB'
  tags: {
    'azd-env-name': uniqueString(resourceGroup().id)
    purpose: 'hackathon'
    project: 'ground-truth-curation'
    defaultExperience: 'Core (SQL)'
    'hidden-workload-type': 'Development/Testing'
  }
  properties: {
    databaseAccountOfferType: 'Standard'
    publicNetworkAccess: 'Enabled'
    enableAutomaticFailover: true
    enableMultipleWriteLocations: false
    isVirtualNetworkFilterEnabled: false
    networkAclBypass: 'None'
    disableKeyBasedMetadataWriteAccess: false
    disableLocalAuth: false
    enableFreeTier: false
    enableAnalyticalStorage: false
    analyticalStorageConfiguration: {
      schemaType: 'WellDefined'
    }
    minimalTlsVersion: 'Tls12'
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
      maxIntervalInSeconds: 5
      maxStalenessPrefix: 100
    }
    capabilities: [
      {
        name: 'EnableServerless'
      }
    ]
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    backupPolicy: {
      type: 'Periodic'
      periodicModeProperties: {
        backupIntervalInMinutes: 240
        backupRetentionIntervalInHours: 8
        backupStorageRedundancy: 'Geo'
      }
    }
    capacity: {
      totalThroughputLimit: 4000
    }
  }
}

resource cosmosPrivateEndpoint 'Microsoft.Network/privateEndpoints@2023-09-01' = {
  name: '${resourceNamePrefix}-cosmos-pe'
  location: location
  properties: {
    subnet: {
      id: dataSubnetId
    }
    privateLinkServiceConnections: [
      {
        name: '${resourceNamePrefix}-cosmos-pls'
        properties: {
          privateLinkServiceId: cosmosDbAccount.id
          groupIds: [
            'Sql'
          ]
        }
      }
    ]
  }

  resource dnsZoneGroup 'privateDnsZoneGroups@2023-09-01' = {
    name: 'cosmos-zone-group'
    properties: {
      privateDnsZoneConfigs: [
        {
          name: 'cosmosDnsConfig'
          properties: {
            privateDnsZoneId: cosmosPrivateDnsZoneId
          }
        }
      ]
    }
  }
}

output accountName string = cosmosDbAccount.name
output documentEndpoint string = cosmosDbAccount.properties.documentEndpoint
output resourceId string = cosmosDbAccount.id
output databaseName string = cosmosDbDatabaseName
output privateEndpointId string = cosmosPrivateEndpoint.id
