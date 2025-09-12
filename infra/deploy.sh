#!/bin/bash

# Ground Truth Curation App - Azure Infrastructure Deployment Script
# This script deploys the Azure SQL Database infrastructure for the hackathon

set -e  # Exit on any error

# Configuration
RESOURCE_GROUP="ground-truth-app-rg"
LOCATION="westus2"
TEMPLATE_FILE="main.bicep"
PARAMETERS_FILE="main.parameters.json"

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

# Create resource group
echo "📦 Creating resource group '$RESOURCE_GROUP' in '$LOCATION'..."
az group create --name "$RESOURCE_GROUP" --location "$LOCATION" --output table

echo ""
echo "🏗️  Deploying infrastructure..."

# Deploy Bicep template
DEPLOYMENT_NAME="ground-truth-deployment-$(date +%Y%m%d-%H%M%S)"
az deployment group create \
    --resource-group "$RESOURCE_GROUP" \
    --template-file "$TEMPLATE_FILE" \
    --parameters @"$PARAMETERS_FILE" \
    --name "$DEPLOYMENT_NAME" \
    --output table

echo ""
echo "✅ Infrastructure deployment completed!"

# Get deployment outputs
echo "📋 Getting deployment information..."
SQL_SERVER=$(az deployment group show \
    --resource-group "$RESOURCE_GROUP" \
    --name "$DEPLOYMENT_NAME" \
    --query properties.outputs.sqlServerFqdn.value -o tsv)

DATABASE_NAME=$(az deployment group show \
    --resource-group "$RESOURCE_GROUP" \
    --name "$DEPLOYMENT_NAME" \
    --query properties.outputs.databaseName.value -o tsv)

echo ""
echo "🎉 Deployment Summary:"
echo "=================================="
echo "SQL Server: $SQL_SERVER"
echo "Database: $DATABASE_NAME"
echo "Resource Group: $RESOURCE_GROUP"
echo "Location: $LOCATION"
echo ""
echo "📝 Next Steps:"
echo "1. Run the table creation script: create-support-tickets-table.sql"
echo "2. Import CSV data using: python import-csv.py"
echo "3. Update connection strings in your applications"
echo ""
echo "🔧 Connection Details:"
echo "Server: $SQL_SERVER"
echo "Database: $DATABASE_NAME"
echo "Authentication: SQL Server (username/password from parameters file)"
echo ""
echo "⚠️  Security Note: This deployment uses hackathon-friendly settings"
echo "   (open firewall, public access). Review security for production use."