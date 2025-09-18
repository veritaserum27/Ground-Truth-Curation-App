@description('The location where all resources will be deployed')
param location string = resourceGroup().location

@description('The name of the System SQL Server')
param systemSqlServerName string = 'gt-system-data-sql'

@description('The name of the Ground Truth SQL Server')
param groundTruthSqlServerName string = 'ground-truth-curation-sql'

@description('The name of the System Demo SQL Database')
param systemDatabaseName string = 'ManufacturingDataRelDB'

@description('The name of the Ground Truth SQL Database')
param groundTruthDatabaseName string = 'GroundTruthDB'

@description('The administrator username for the SQL Server')
param sqlAdministratorLogin string

@description('The administrator password for the SQL Server')
@secure()
@minLength(8)
@maxLength(128)
param sqlAdministratorPassword string

@description('The SKU for the SQL Database')
param sqlDatabaseSku object = {
  name: 'Basic'
  tier: 'Basic'
  capacity: 5
}

@description('The name of the Azure Cosmos DB account')
param cosmosDbAccountName string = 'gt-system-data-cosmos'

resource systemSqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: systemSqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdministratorLogin
    administratorLoginPassword: sqlAdministratorPassword
    version: '12.0'
    publicNetworkAccess: 'Enabled'
  }
}

// Ground Truth SQL Server  
resource groundTruthSqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: groundTruthSqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdministratorLogin
    administratorLoginPassword: sqlAdministratorPassword
    version: '12.0'
    publicNetworkAccess: 'Enabled'
  }
}

// SQL Database - System Demo
resource systemSqlDatabase 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  parent: systemSqlServer
  name: systemDatabaseName
  location: location
  sku: sqlDatabaseSku
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 2147483648 // 2GB
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    zoneRedundant: false
    readScale: 'Disabled'
    requestedBackupStorageRedundancy: 'Local'
  }
}

// SQL Database - Ground Truth
resource groundTruthSqlDatabase 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  parent: groundTruthSqlServer
  name: groundTruthDatabaseName
  location: location
  sku: sqlDatabaseSku
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 2147483648 // 2GB
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    zoneRedundant: false
    readScale: 'Disabled'
    requestedBackupStorageRedundancy: 'Local'
  }
}

// Firewall rule to allow Azure services - System SQL Server
resource systemFirewallRuleAzure 'Microsoft.Sql/servers/firewallRules@2023-05-01-preview' = {
  parent: systemSqlServer
  name: 'AllowAllWindowsAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// Hackathon firewall rule to allow all IPs - System SQL Server
resource systemFirewallRuleHackathon 'Microsoft.Sql/servers/firewallRules@2023-05-01-preview' = {
  parent: systemSqlServer
  name: 'HackathonOpenAccess'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '255.255.255.255'
  }
}

// Firewall rule to allow Azure services - Ground Truth SQL Server
resource groundTruthFirewallRuleAzure 'Microsoft.Sql/servers/firewallRules@2023-05-01-preview' = {
  parent: groundTruthSqlServer
  name: 'AllowAllWindowsAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// Hackathon firewall rule to allow all IPs - Ground Truth SQL Server
resource groundTruthFirewallRuleHackathon 'Microsoft.Sql/servers/firewallRules@2023-05-01-preview' = {
  parent: groundTruthSqlServer
  name: 'HackathonOpenAccess'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '255.255.255.255'
  }
}

// Azure Cosmos DB Account (Serverless, account only)
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
    // No virtual network or IP rules
    networkAclBypass: 'None'
    // No network ACL bypass resource IDs
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
    // failoverPolicies is read-only and not settable
    backupPolicy: {
      type: 'Periodic'
      periodicModeProperties: {
        backupIntervalInMinutes: 240
        backupRetentionIntervalInHours: 8
        backupStorageRedundancy: 'Geo'
      }
    }
    // No CORS rules
    capacity: {
      totalThroughputLimit: 4000
    }
  }
}

output systemSqlServerName string = systemSqlServer.name
output systemSqlServerFqdn string = systemSqlServer.properties.fullyQualifiedDomainName
output groundTruthSqlServerName string = groundTruthSqlServer.name
output groundTruthSqlServerFqdn string = groundTruthSqlServer.properties.fullyQualifiedDomainName
output systemDatabaseName string = systemSqlDatabase.name
output groundTruthDatabaseName string = groundTruthSqlDatabase.name
output connectionStringTemplateSystem string = 'Server=tcp:${systemSqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${systemDatabaseName};Persist Security Info=False;User ID=${sqlAdministratorLogin};Password=<your-password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
output connectionStringTemplateGroundTruth string = 'Server=tcp:${groundTruthSqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${groundTruthDatabaseName};Persist Security Info=False;User ID=${sqlAdministratorLogin};Password=<your-password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'

// Cosmos DB outputs
output cosmosDbAccountName string = cosmosDbAccount.name
output cosmosDbAccountEndpoint string = cosmosDbAccount.properties.documentEndpoint
output cosmosDbResourceId string = cosmosDbAccount.id
