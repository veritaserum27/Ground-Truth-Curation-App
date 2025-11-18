#!/bin/bash

# Ground Truth Curation App - Azure Infrastructure Deployment Script
# This script deploys the Azure infrastructure for the hackathon

set -e  # Exit on any error

if [ -f .env ]; then
    echo "📄 Loading environment variables from .env file..."
    export $(cat .env | grep -v '^#' | xargs)
else
    echo "⚠️  No .env file found. Please create one with RESOURCE_NAME_PREFIX (and any other overrides)."
    echo "   Example: echo 'RESOURCE_NAME_PREFIX=brleight' > .env"
    exit 1
fi

if [ -z "$RESOURCE_NAME_PREFIX" ]; then
    echo "❌ RESOURCE_NAME_PREFIX is required. Set it in your environment or .env file."
    exit 1
fi

# Configuration
RESOURCE_GROUP="$RESOURCE_NAME_PREFIX-ground-truth-app-rg"
LOCATION="centralus"
TEMPLATE_FILE="main.bicep"
PARAMETERS_FILE="main.parameters.json"
DEPLOYMENT_NAME="$RESOURCE_NAME_PREFIX-ground-truth-deployment"

echo "🚀 Starting deployment of Ground Truth Curation App infrastructure..."
echo "📍 Resource Group: $RESOURCE_GROUP"
echo "🌍 Location: $LOCATION"
echo ""

# Check if Azure CLI is installed
if ! command -v az &> /dev/null; then
    echo "❌ Azure CLI is not installed. Please install it first:"
    echo "   https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
    exit 1
fi

# Check if user is logged in
echo "🔐 Checking Azure CLI authentication..."
if ! az account show &> /dev/null; then
    echo "❌ Please log in to Azure CLI first:"
    echo "   az login"
    exit 1
fi

# Show current subscription
SUBSCRIPTION=$(az account show --query name -o tsv)
echo "✅ Logged in to Azure subscription: $SUBSCRIPTION"
echo ""

echo "👤 Resolving Azure AD administrator from current Azure CLI context..."
ACCOUNT_USER_TYPE=$(az account show --query user.type -o tsv)
ACCOUNT_USER_PRINCIPAL=$(az account show --query user.name -o tsv)
ACCOUNT_TENANT_ID=$(az account show --query tenantId -o tsv)

case "$ACCOUNT_USER_TYPE" in
    user)
        if ! az ad signed-in-user show &> /dev/null; then
            echo "❌ Unable to resolve the signed-in Azure AD user. Ensure you have permission to query Microsoft Graph (az ad signed-in-user show)."
            exit 1
        fi
        SQL_AAD_LOGIN=$(az ad signed-in-user show --query userPrincipalName -o tsv)
        SQL_AAD_OBJECT_ID=$(az ad signed-in-user show --query id -o tsv)
        SQL_AAD_PRINCIPAL_TYPE="User"
        ;;
    servicePrincipal)
        if [ -z "$ACCOUNT_USER_PRINCIPAL" ]; then
            echo "❌ Service principal ID not available from az account show."
            exit 1
        fi
        if ! az ad sp show --id "$ACCOUNT_USER_PRINCIPAL" &> /dev/null; then
            echo "❌ Unable to resolve service principal $ACCOUNT_USER_PRINCIPAL. Verify the service principal exists and you have appropriate permissions."
            exit 1
        fi
        SQL_AAD_LOGIN=$(az ad sp show --id "$ACCOUNT_USER_PRINCIPAL" --query displayName -o tsv)
        SQL_AAD_OBJECT_ID=$(az ad sp show --id "$ACCOUNT_USER_PRINCIPAL" --query id -o tsv)
        SQL_AAD_PRINCIPAL_TYPE="ServicePrincipal"
        ;;
    *)
        echo "❌ Unsupported Azure account user type '$ACCOUNT_USER_TYPE'. Please deploy using a user or service principal context."
        exit 1
        ;;
esac

if [ -z "$SQL_AAD_LOGIN" ] || [ -z "$SQL_AAD_OBJECT_ID" ]; then
    echo "❌ Failed to resolve Azure AD administrator information from the current Azure CLI login."
    exit 1
fi

echo "✅ Using Azure AD administrator: $SQL_AAD_LOGIN ($SQL_AAD_PRINCIPAL_TYPE)"
echo ""

if [ -z "$ADMIN_CLIENT_IP" ] && [ -z "$SKIP_ADMIN_IP_DISCOVERY" ]; then
    echo "🌐 Detecting current public IP for SQL firewall rule..."
    ADMIN_CLIENT_IP=$(curl -s https://ifconfig.me || curl -s https://api.ipify.org || true)
elif [ -n "$SKIP_ADMIN_IP_DISCOVERY" ]; then
    echo "ℹ️  Skipping admin IP autodetection (SKIP_ADMIN_IP_DISCOVERY set)."
fi

if [ -n "$ADMIN_CLIENT_IP" ]; then
    ADMIN_CLIENT_IP=$(echo "$ADMIN_CLIENT_IP" | tr -d '\n\r')
    echo "✅ Using admin client IP for SQL firewall: $ADMIN_CLIENT_IP"
else
    echo "⚠️  Admin client IP not provided. SQL servers will rely on private endpoints only."
    echo "   Set ADMIN_CLIENT_IP in .env or export it before running deploy.sh to add a firewall rule."
    echo "   Alternatively, set SKIP_ADMIN_IP_DISCOVERY=1 to suppress this message."
fi

# Create resource group (will skip if exists)
echo "📦 Creating resource group '$RESOURCE_GROUP' in '$LOCATION'..."
az group create --name "$RESOURCE_GROUP" --location "$LOCATION" --output table

echo ""
echo "🏗️  Deploying infrastructure..."

# Console log parameters before attempting deployment
echo "🔧 Deployment Parameters:"
echo "  Template File: $TEMPLATE_FILE"
echo "  Parameters File: $PARAMETERS_FILE"
echo "  sqlAadAdministratorLogin: $SQL_AAD_LOGIN"
echo "  sqlAadAdministratorObjectId: $SQL_AAD_OBJECT_ID"
echo "  sqlAadAdministratorPrincipalType: $SQL_AAD_PRINCIPAL_TYPE"
echo "  sqlAadAdministratorTenantId: $ACCOUNT_TENANT_ID"
echo "  resourceNamePrefix: $RESOURCE_NAME_PREFIX"
echo "  adminClientIpAddress: ${ADMIN_CLIENT_IP:-<not set>}"
echo "  Resource Group: $RESOURCE_GROUP"
echo "  Location: $LOCATION"
echo "  Deployment Name: $DEPLOYMENT_NAME"
echo ""

# Deploy Bicep template with incremental mode (skips existing resources)
az deployment group create \
    --resource-group "$RESOURCE_GROUP" \
    --template-file "$TEMPLATE_FILE" \
    --parameters @"$PARAMETERS_FILE" \
    --parameters sqlAadAdministratorLogin="$SQL_AAD_LOGIN" \
                 sqlAadAdministratorObjectId="$SQL_AAD_OBJECT_ID" \
                 sqlAadAdministratorPrincipalType="$SQL_AAD_PRINCIPAL_TYPE" \
                 sqlAadAdministratorTenantId="$ACCOUNT_TENANT_ID" \
                 resourceNamePrefix="$RESOURCE_NAME_PREFIX" \
                 adminClientIpAddress="${ADMIN_CLIENT_IP:-}" \
    --name "$DEPLOYMENT_NAME" \
    --mode Incremental \
    --output table

echo ""
echo "✅ Infrastructure deployment completed!"

# Get deployment outputs
echo "📋 Getting deployment information..."
SYSTEM_SQL_SERVER=$(az deployment group show \
    --resource-group "$RESOURCE_GROUP" \
    --name "$DEPLOYMENT_NAME" \
    --query properties.outputs.systemSqlServerFqdn.value -o tsv)

GROUND_TRUTH_SQL_SERVER=$(az deployment group show \
    --resource-group "$RESOURCE_GROUP" \
    --name "$DEPLOYMENT_NAME" \
    --query properties.outputs.groundTruthSqlServerFqdn.value -o tsv)

SYSTEM_DATABASE=$(az deployment group show \
    --resource-group "$RESOURCE_GROUP" \
    --name "$DEPLOYMENT_NAME" \
    --query properties.outputs.systemDatabaseName.value -o tsv)

GROUND_TRUTH_DATABASE=$(az deployment group show \
    --resource-group "$RESOURCE_GROUP" \
    --name "$DEPLOYMENT_NAME" \
    --query properties.outputs.groundTruthDatabaseName.value -o tsv)

COSMOS_ACCOUNT=$(az deployment group show \
    --resource-group "$RESOURCE_GROUP" \
    --name "$DEPLOYMENT_NAME" \
    --query properties.outputs.cosmosDbAccountName.value -o tsv)

COSMOS_ENDPOINT=$(az deployment group show \
    --resource-group "$RESOURCE_GROUP" \
    --name "$DEPLOYMENT_NAME" \
    --query properties.outputs.cosmosDbAccountEndpoint.value -o tsv)

COSMOS_DATABASE=$(az deployment group show \
    --resource-group "$RESOURCE_GROUP" \
    --name "$DEPLOYMENT_NAME" \
    --query properties.outputs.cosmosDbDatabaseName.value -o tsv)

DATA_VIRTUAL_NETWORK=$(az deployment group show \
    --resource-group "$RESOURCE_GROUP" \
    --name "$DEPLOYMENT_NAME" \
    --query properties.outputs.dataVirtualNetworkName.value -o tsv)

DATA_SUBNET_RESOURCE_ID=$(az deployment group show \
    --resource-group "$RESOURCE_GROUP" \
    --name "$DEPLOYMENT_NAME" \
    --query properties.outputs.dataSubnetResourceId.value -o tsv)

SYSTEM_SQL_PRIVATE_ENDPOINT=$(az deployment group show \
    --resource-group "$RESOURCE_GROUP" \
    --name "$DEPLOYMENT_NAME" \
    --query properties.outputs.systemSqlPrivateEndpointId.value -o tsv)

GROUND_TRUTH_SQL_PRIVATE_ENDPOINT=$(az deployment group show \
    --resource-group "$RESOURCE_GROUP" \
    --name "$DEPLOYMENT_NAME" \
    --query properties.outputs.groundTruthSqlPrivateEndpointId.value -o tsv)

COSMOS_PRIVATE_ENDPOINT=$(az deployment group show \
    --resource-group "$RESOURCE_GROUP" \
    --name "$DEPLOYMENT_NAME" \
    --query properties.outputs.cosmosPrivateEndpointId.value -o tsv)

ADMIN_FIREWALL_IP=$(az deployment group show \
    --resource-group "$RESOURCE_GROUP" \
    --name "$DEPLOYMENT_NAME" \
    --query properties.outputs.adminClientIpAddress.value -o tsv)

SQL_FIREWALL_APPLIED=$(az deployment group show \
    --resource-group "$RESOURCE_GROUP" \
    --name "$DEPLOYMENT_NAME" \
    --query properties.outputs.sqlFirewallRuleApplied.value -o tsv)

# Get Cosmos DB primary key for authentication
COSMOS_PRIMARY_KEY=$(az cosmosdb keys list \
    --resource-group "$RESOURCE_GROUP" \
    --name "$COSMOS_ACCOUNT" \
    --type keys \
    --query primaryMasterKey -o tsv)

# Update .env file with deployed infrastructure details
echo "📝 Updating .env file with deployment details..."
if [ -f .env ]; then
    # Create a backup
    cp .env .env.backup

    # Update SQL Server connection details (for CSV imports)
    sed -i.tmp "s|^DB_SERVER=.*|DB_SERVER=$SYSTEM_SQL_SERVER|" .env
    sed -i.tmp "s|^DB_DATABASE=.*|DB_DATABASE=$SYSTEM_DATABASE|" .env

    # Add or update additional deployment information
    # Remove any existing deployment info section
    sed -i.tmp '/^# === DEPLOYMENT INFO ===/,/^# === END DEPLOYMENT INFO ===/d' .env

    # Add new deployment info section
    cat >> .env << EOF

# === DEPLOYMENT INFO ===
# Auto-generated by deploy.sh on $(date)

# System SQL Server (for support tickets and demo data)
SYSTEM_SQL_SERVER=$SYSTEM_SQL_SERVER
SYSTEM_DATABASE=$SYSTEM_DATABASE

# Ground Truth SQL Server (for curation workflow)
GROUND_TRUTH_SQL_SERVER=$GROUND_TRUTH_SQL_SERVER
GROUND_TRUTH_DATABASE=$GROUND_TRUTH_DATABASE

# Cosmos DB (for manufacturing defects)
COSMOS_ACCOUNT=$COSMOS_ACCOUNT
COSMOS_ENDPOINT=$COSMOS_ENDPOINT
COSMOS_DATABASE=$COSMOS_DATABASE
COSMOS_PRIMARY_KEY=$COSMOS_PRIMARY_KEY
COSMOS_CONNECTION_STRING=AccountEndpoint=$COSMOS_ENDPOINT;AccountKey=$COSMOS_PRIMARY_KEY;Database=$COSMOS_DATABASE;

# Networking
DATA_VIRTUAL_NETWORK=$DATA_VIRTUAL_NETWORK
DATA_SUBNET_RESOURCE_ID=$DATA_SUBNET_RESOURCE_ID
SYSTEM_SQL_PRIVATE_ENDPOINT=$SYSTEM_SQL_PRIVATE_ENDPOINT
GROUND_TRUTH_SQL_PRIVATE_ENDPOINT=$GROUND_TRUTH_SQL_PRIVATE_ENDPOINT
COSMOS_PRIVATE_ENDPOINT=$COSMOS_PRIVATE_ENDPOINT
SQL_FIREWALL_IP=$ADMIN_FIREWALL_IP
SQL_FIREWALL_RULE_ENABLED=$SQL_FIREWALL_APPLIED

# Azure Resource Details
AZURE_RESOURCE_GROUP=$RESOURCE_GROUP
AZURE_LOCATION=$LOCATION
DEPLOYMENT_NAME=$DEPLOYMENT_NAME
# === END DEPLOYMENT INFO ===
EOF

    rm -f .env.tmp

    echo "✅ Updated .env file with:"
    echo "   DB_SERVER=$SYSTEM_SQL_SERVER"
    echo "   DB_DATABASE=$SYSTEM_DATABASE"
    echo "   + Cosmos DB connection details"
    echo "   + All deployment resource details"
    echo "   📄 Backup saved as .env.backup"
else
    echo "⚠️  .env file not found - skipping update"
fi

echo ""
echo "🎉 Deployment Summary:"
echo "=================================="
echo "System SQL Server: $SYSTEM_SQL_SERVER"
echo "System Database: $SYSTEM_DATABASE"
echo "Ground Truth SQL Server: $GROUND_TRUTH_SQL_SERVER"
echo "Ground Truth Database: $GROUND_TRUTH_DATABASE"
echo "Cosmos DB Account: $COSMOS_ACCOUNT"
echo "Cosmos DB Endpoint: $COSMOS_ENDPOINT"
echo "Cosmos DB Database: $COSMOS_DATABASE"
echo "Cosmos DB Primary Key: ${COSMOS_PRIMARY_KEY:0:20}..." # Show only first 20 chars for security
echo "Virtual Network: $DATA_VIRTUAL_NETWORK"
echo "Data Subnet Resource ID: $DATA_SUBNET_RESOURCE_ID"
echo "System SQL Private Endpoint: $SYSTEM_SQL_PRIVATE_ENDPOINT"
echo "Ground Truth SQL Private Endpoint: $GROUND_TRUTH_SQL_PRIVATE_ENDPOINT"
echo "Cosmos Private Endpoint: $COSMOS_PRIVATE_ENDPOINT"
if [ "$SQL_FIREWALL_APPLIED" = "true" ]; then
    echo "SQL Firewall Rule IP: $ADMIN_FIREWALL_IP"
else
    echo "SQL Firewall Rule IP: (none - private endpoints only)"
fi
echo "Resource Group: $RESOURCE_GROUP"
echo "Location: $LOCATION"
echo ""
echo "📝 Next Steps:"
echo "1. Run the table creation script: create-support-tickets-table.sql"
echo "2. Import CSV data using: python import-support-tickets-csv.py"
echo "3. Upload defects data using: python upload-defects-csv.py"
echo "4. Update connection strings in your applications"
echo ""
echo "🔧 Connection Details:"
echo "System SQL Server: $SYSTEM_SQL_SERVER"
echo "System Database: $SYSTEM_DATABASE (for support tickets)"
echo "Ground Truth SQL Server: $GROUND_TRUTH_SQL_SERVER"
echo "Ground Truth Database: $GROUND_TRUTH_DATABASE (for curation data)"
echo "Cosmos DB Endpoint: $COSMOS_ENDPOINT"
echo "Cosmos DB Database: $COSMOS_DATABASE (for manufacturing defects)"
echo "Authentication: SQL Server (username/password), Cosmos DB (primary key in .env)"
echo ""
echo "💰 Billing Note: Cosmos DB is configured for serverless (consumption-based)"
echo "   You only pay for Request Units (RUs) consumed and storage used."
echo ""
echo "⚠️  Security Note: This deployment uses hackathon-friendly settings"
echo "   (open firewall, public access). Review security for production use."
