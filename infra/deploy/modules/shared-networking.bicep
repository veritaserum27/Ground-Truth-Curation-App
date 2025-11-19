@description('Builds the shared virtual network, subnet, and private DNS zones for data services.')
param location string
param resourceNamePrefix string
param virtualNetworkName string
param vnetAddressPrefixes array
param dataSubnetName string
param dataSubnetPrefix string
param appServicesSubnetName string
param appServicesSubnetPrefix string
param cosmosPrivateDnsZoneName string = 'privatelink.documents.azure.com'

resource dataVirtualNetwork 'Microsoft.Network/virtualNetworks@2023-09-01' = {
  name: virtualNetworkName
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: vnetAddressPrefixes
    }
    subnets: [
      {
        name: dataSubnetName
        properties: {
          addressPrefix: dataSubnetPrefix
          privateEndpointNetworkPolicies: 'Disabled'
          privateLinkServiceNetworkPolicies: 'Enabled'
        }
      }
      {
        name: appServicesSubnetName
        properties: {
          addressPrefix: appServicesSubnetPrefix
          delegations: [
            {
              name: 'delegation'
              properties: {
                serviceName: 'Microsoft.Web/serverFarms'
              }
            }
          ]
          privateEndpointNetworkPolicies: 'Enabled'
          privateLinkServiceNetworkPolicies: 'Enabled'
        }
      }
    ]
  }
}

resource sqlPrivateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: 'privatelink${environment().suffixes.sqlServerHostname}'
  location: 'global'

  resource sqlZoneVnetLink 'virtualNetworkLinks@2020-06-01' = {
    name: '${resourceNamePrefix}-sql-vnet-link'
    location: 'global'
    properties: {
      virtualNetwork: {
        id: dataVirtualNetwork.id
      }
      registrationEnabled: false
    }
  }
}

resource cosmosPrivateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: cosmosPrivateDnsZoneName
  location: 'global'

  resource cosmosZoneVnetLink 'virtualNetworkLinks@2020-06-01' = {
    name: '${resourceNamePrefix}-cosmos-vnet-link'
    location: 'global'
    properties: {
      virtualNetwork: {
        id: dataVirtualNetwork.id
      }
      registrationEnabled: false
    }
  }
}

output virtualNetworkId string = dataVirtualNetwork.id
output virtualNetworkName string = dataVirtualNetwork.name
output dataSubnetId string = resourceId('Microsoft.Network/virtualNetworks/subnets', virtualNetworkName, dataSubnetName)
output appServicesSubnetId string = resourceId(
  'Microsoft.Network/virtualNetworks/subnets',
  virtualNetworkName,
  appServicesSubnetName
)
output sqlPrivateDnsZoneId string = sqlPrivateDnsZone.id
output cosmosPrivateDnsZoneId string = cosmosPrivateDnsZone.id
