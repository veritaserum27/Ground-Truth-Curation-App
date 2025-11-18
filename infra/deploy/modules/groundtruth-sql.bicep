@description('Deploys the Ground Truth SQL Server, database, firewall rule, and private endpoint.')
param location string
param resourceNamePrefix string
param serverName string
param databaseName string
param sqlDatabaseSku object
param sqlAadAdministratorLogin string
param sqlAadAdministratorObjectId string
param sqlAadAdministratorPrincipalType string
param sqlAadAdministratorTenantId string
param adminClientIpAddress string = ''
param dataSubnetId string
param sqlPrivateDnsZoneId string

resource groundTruthSqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: serverName
  location: location
  properties: {
    version: '12.0'
    publicNetworkAccess: 'Enabled'
    administrators: {
      administratorType: 'ActiveDirectory'
      login: sqlAadAdministratorLogin
      principalType: sqlAadAdministratorPrincipalType
      sid: sqlAadAdministratorObjectId
      tenantId: sqlAadAdministratorTenantId
      azureADOnlyAuthentication: true
    }
  }
}

resource groundTruthSqlDatabase 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  parent: groundTruthSqlServer
  name: databaseName
  location: location
  sku: sqlDatabaseSku
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 2147483648
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    zoneRedundant: false
    readScale: 'Disabled'
    requestedBackupStorageRedundancy: 'Local'
  }
}

resource groundTruthFirewallRuleAdmin 'Microsoft.Sql/servers/firewallRules@2023-05-01-preview' = if (!empty(adminClientIpAddress)) {
  parent: groundTruthSqlServer
  name: 'AdminClientIp'
  properties: {
    startIpAddress: adminClientIpAddress
    endIpAddress: adminClientIpAddress
  }
}

resource groundTruthSqlPrivateEndpoint 'Microsoft.Network/privateEndpoints@2023-09-01' = {
  name: '${resourceNamePrefix}-groundtruth-sql-pe'
  location: location
  properties: {
    subnet: {
      id: dataSubnetId
    }
    privateLinkServiceConnections: [
      {
        name: '${resourceNamePrefix}-groundtruth-sql-pls'
        properties: {
          privateLinkServiceId: groundTruthSqlServer.id
          groupIds: [
            'sqlServer'
          ]
        }
      }
    ]
  }

  resource dnsZoneGroup 'privateDnsZoneGroups@2023-09-01' = {
    name: 'sql-zone-group'
    properties: {
      privateDnsZoneConfigs: [
        {
          name: 'sqlDnsConfig'
          properties: {
            privateDnsZoneId: sqlPrivateDnsZoneId
          }
        }
      ]
    }
  }
}

output serverName string = groundTruthSqlServer.name
output serverFqdn string = groundTruthSqlServer.properties.fullyQualifiedDomainName
output databaseName string = groundTruthSqlDatabase.name
output privateEndpointId string = groundTruthSqlPrivateEndpoint.id
output firewallRuleApplied bool = !empty(adminClientIpAddress)
output firewallRuleIp string = adminClientIpAddress
