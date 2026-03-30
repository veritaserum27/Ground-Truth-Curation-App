#!/bin/bash

# Setup GitHub OIDC Authentication for Azure Deployments
# This script creates an Azure AD App Registration with Federated Identity Credentials
# to enable GitHub Actions to deploy to Azure using OIDC (no secrets required)

set -e  # Exit on any error

# Configuration
GITHUB_REPO_OWNER="veritaserum27"
GITHUB_REPO_NAME="Ground-Truth-Curation-App"
APP_NAME="github-oidc-$GITHUB_REPO_NAME"
ENVIRONMENTS=("staging" "production")

echo "🔐 Setting up GitHub OIDC Authentication for Azure"
echo "=================================================="
echo ""
echo "GitHub Repository: $GITHUB_REPO_OWNER/$GITHUB_REPO_NAME"
echo "App Registration Name: $APP_NAME"
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

# Get current subscription details
SUBSCRIPTION_ID=$(az account show --query id -o tsv)
SUBSCRIPTION_NAME=$(az account show --query name -o tsv)
TENANT_ID=$(az account show --query tenantId -o tsv)

echo "✅ Authenticated to Azure"
echo "   Subscription: $SUBSCRIPTION_NAME"
echo "   Subscription ID: $SUBSCRIPTION_ID"
echo "   Tenant ID: $TENANT_ID"
echo ""

# Check if app registration already exists
echo "🔍 Checking if app registration already exists..."
EXISTING_APP=$(az ad app list --display-name "$APP_NAME" --query "[0].appId" -o tsv)

if [ -n "$EXISTING_APP" ]; then
    echo "✅ App registration '$APP_NAME' already exists"
    APP_ID="$EXISTING_APP"
    echo "   App (Client) ID: $APP_ID"
else
    echo "📝 Creating app registration '$APP_NAME'..."
    APP_ID=$(az ad app create \
        --display-name "$APP_NAME" \
        --query appId -o tsv)
    
    echo "✅ App registration created"
    echo "   App (Client) ID: $APP_ID"
    
    # Wait a moment for the app to be fully created
    sleep 5
fi

# Get the service principal (create if it doesn't exist)
echo ""
echo "🔍 Checking service principal..."
SP_ID=$(az ad sp list --filter "appId eq '$APP_ID'" --query "[0].id" -o tsv)

if [ -z "$SP_ID" ]; then
    echo "📝 Creating service principal..."
    SP_ID=$(az ad sp create --id "$APP_ID" --query id -o tsv)
    echo "✅ Service principal created"
    
    # Wait for service principal to propagate
    sleep 10
else
    echo "✅ Service principal already exists"
fi

echo "   Service Principal ID: $SP_ID"

# Configure federated identity credentials for each environment
echo ""
echo "🔗 Configuring Federated Identity Credentials..."

for ENV in "${ENVIRONMENTS[@]}"; do
    CREDENTIAL_NAME="github-$ENV"
    SUBJECT="repo:$GITHUB_REPO_OWNER/$GITHUB_REPO_NAME:environment:$ENV"
    
    echo ""
    echo "   Environment: $ENV"
    echo "   Subject: $SUBJECT"
    
    # Check if credential already exists
    EXISTING_CRED=$(az ad app federated-credential list \
        --id "$APP_ID" \
        --query "[?name=='$CREDENTIAL_NAME'].name" -o tsv 2>/dev/null || echo "")
    
    if [ -n "$EXISTING_CRED" ]; then
        echo "   ⚠️  Federated credential '$CREDENTIAL_NAME' already exists, skipping..."
    else
        # Create federated credential
        az ad app federated-credential create \
            --id "$APP_ID" \
            --parameters "{
                \"name\": \"$CREDENTIAL_NAME\",
                \"issuer\": \"https://token.actions.githubusercontent.com\",
                \"subject\": \"$SUBJECT\",
                \"description\": \"GitHub Actions deployment for $ENV environment\",
                \"audiences\": [
                    \"api://AzureADTokenExchange\"
                ]
            }" > /dev/null
        
        echo "   ✅ Federated credential created for $ENV"
    fi
done

# Assign Contributor role to the service principal on the subscription
echo ""
echo "🔑 Assigning Contributor role to service principal..."

# Check if role assignment already exists
EXISTING_ROLE=$(az role assignment list \
    --assignee "$SP_ID" \
    --role "Contributor" \
    --scope "/subscriptions/$SUBSCRIPTION_ID" \
    --query '[0].id' -o tsv 2>/dev/null || echo "")

if [ -n "$EXISTING_ROLE" ]; then
    echo "✅ Contributor role already assigned"
else
    # Assign the role
    az role assignment create \
        --assignee "$SP_ID" \
        --role "Contributor" \
        --scope "/subscriptions/$SUBSCRIPTION_ID" > /dev/null
    
    echo "✅ Contributor role assigned to subscription"
fi

# Get current user details for SQL AAD admin configuration
echo ""
echo "🔍 Getting current user details for SQL AAD admin configuration..."
USER_PRINCIPAL_NAME=$(az ad signed-in-user show --query userPrincipalName -o tsv 2>/dev/null || echo "")
USER_OBJECT_ID=$(az ad signed-in-user show --query id -o tsv 2>/dev/null || echo "")

# Output the required GitHub secrets
echo ""
echo "=================================================="
echo "✅ GitHub OIDC Setup Complete!"
echo "=================================================="
echo ""
echo "📋 Add the following SECRETS to your GitHub repository:"
echo "   (Settings → Secrets and variables → Actions → New repository secret)"
echo ""
echo "   Secret Name: AZURE_CLIENT_ID"
echo "   Value: $APP_ID"
echo ""
echo "   Secret Name: AZURE_TENANT_ID"
echo "   Value: $TENANT_ID"
echo ""
echo "   Secret Name: AZURE_SUBSCRIPTION_ID"
echo "   Value: $SUBSCRIPTION_ID"
echo ""
echo "=================================================="
echo ""
echo "📋 Add the following VARIABLES to your GitHub repository:"
echo "   (Settings → Secrets and variables → Actions → Variables tab)"
echo ""
if [ -n "$USER_PRINCIPAL_NAME" ] && [ -n "$USER_OBJECT_ID" ]; then
    echo "   ✅ SQL AAD Admin Configuration (User Account)"
    echo ""
    echo "   Variable Name: SQL_AAD_ADMIN_LOGIN"
    echo "   Value: $USER_PRINCIPAL_NAME"
    echo ""
    echo "   Variable Name: SQL_AAD_ADMIN_OBJECT_ID"
    echo "   Value: $USER_OBJECT_ID"
    echo ""
    echo "   Variable Name: SQL_AAD_ADMIN_PRINCIPAL_TYPE"
    echo "   Value: User"
    echo ""
    echo "   ℹ️  Using your user account as SQL admin (recommended for sandboxes)"
    echo "      This works without requiring Graph API permissions on the service principal."
else
    echo "   ⚠️  Could not retrieve current user details automatically."
    echo ""
    echo "   Run this command to get your user details:"
    echo "   az ad signed-in-user show --query \"{login:userPrincipalName, objectId:id}\" -o table"
    echo ""
    echo "   Then set these variables:"
    echo "     SQL_AAD_ADMIN_LOGIN: <your email from above>"
    echo "     SQL_AAD_ADMIN_OBJECT_ID: <your object ID from above>"
    echo "     SQL_AAD_ADMIN_PRINCIPAL_TYPE: User"
    echo ""
    echo "   ℹ️  Note: Using your user account as SQL admin is recommended for sandbox"
    echo "      subscriptions where Graph API permissions cannot be granted to the"
    echo "      service principal. The service principal will still handle all"
    echo "      infrastructure deployments via OIDC."
fi
echo ""
echo "=================================================="
echo ""
echo "🔗 GitHub Secrets URL:"
echo "   https://github.com/$GITHUB_REPO_OWNER/$GITHUB_REPO_NAME/settings/secrets/actions"
echo ""
echo "🔗 GitHub Variables URL:"
echo "   https://github.com/$GITHUB_REPO_OWNER/$GITHUB_REPO_NAME/settings/variables/actions"
echo ""
echo "📝 Note: You also need to create GitHub Environments for:"
for ENV in "${ENVIRONMENTS[@]}"; do
    echo "   - $ENV"
done
echo ""
echo "   GitHub Environments URL:"
echo "   https://github.com/$GITHUB_REPO_OWNER/$GITHUB_REPO_NAME/settings/environments"
echo ""
echo "✅ Once secrets and variables are added, your GitHub Actions workflows can deploy to Azure!"
