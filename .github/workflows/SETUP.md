# GitHub Actions CI/CD Setup Guide

This guide walks you through setting up GitHub Actions workflows for automated infrastructure and application deployments to Azure.

## Overview

The CI/CD setup includes:

- **Infrastructure Deployment**: Automatically deploys Azure resources using Bicep templates
- **Backend Deployment**: Builds and deploys the .NET 8 API to Azure App Service
- **Frontend Deployment**: Builds and deploys the React application to Azure App Service
- **Data Seeding**: Optional manual workflow to seed databases

## Prerequisites

- Azure subscription with appropriate permissions
- GitHub repository admin access
- Azure CLI installed locally ([Install Guide](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli))
- Permissions to create service principals in your Azure AD tenant

## Step 1: Create Azure Service Principal with Federated Credentials

We'll use OpenID Connect (OIDC) for secure, keyless authentication from GitHub to Azure.

### 1.1 Login to Azure

```bash
az login
az account set --subscription "<your-subscription-id>"
```

### 1.2 Get Your Subscription Details

```bash
# Get subscription ID
SUBSCRIPTION_ID=$(az account show --query id -o tsv)
echo "Subscription ID: $SUBSCRIPTION_ID"

# Get tenant ID
TENANT_ID=$(az account show --query tenantId -o tsv)
echo "Tenant ID: $TENANT_ID"
```

### 1.3 Create Service Principal

Replace `<your-org>` and `<your-repo>` with your GitHub organization/username and repository name:

```bash
# Set variables
GITHUB_ORG="<your-org>"
GITHUB_REPO="<your-repo>"
SP_NAME="sp-github-groundtruth-cicd"

# Create Azure AD application
APP_ID=$(az ad app create --display-name "$SP_NAME" --query appId -o tsv)
echo "Application ID: $APP_ID"

# Create service principal for the application
az ad sp create --id $APP_ID

# Assign Contributor role
az role assignment create \
  --assignee $APP_ID \
  --role Contributor \
  --scope /subscriptions/$SUBSCRIPTION_ID

# Reset credentials with 360-day expiry
az ad app credential reset \
  --id $APP_ID \
  --end-date $(date -u -d "360 days" '+%Y-%m-%dT%H:%M:%SZ')

# Save the APP_ID as CLIENT_ID for federated credentials
CLIENT_ID=$APP_ID
echo "Client ID: $CLIENT_ID"
```

**Important**: Save the credential reset output. You'll need the password for initial testing (though federated credentials won't use it in production).

**To reset credentials for an existing service principal:**

```bash
# Reset credentials with 360-day expiry
az ad sp credential reset \
  --id $CLIENT_ID \
  --end-date $(date -u -d "360 days" '+%Y-%m-%dT%H:%M:%SZ')
```

### 1.4 Get the Application (Client) ID

```bash
# Get the application ID of the service principal
CLIENT_ID=$(az ad sp list --display-name "$SP_NAME" --query "[0].appId" -o tsv)
echo "Client ID: $CLIENT_ID"
```

### 1.5 Configure Federated Credentials for GitHub

You have several options for configuring federated credentials. Choose the pattern(s) that match your workflow needs:

**Option 1: Specific branch (main)**

```bash
az ad app federated-credential create \
  --id $CLIENT_ID \
  --parameters '{
    "name": "github-deploy-main",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:'"$GITHUB_ORG"'/'"$GITHUB_REPO"':ref:refs/heads/main",
    "audiences": ["api://AzureADTokenExchange"]
  }'
```

**Option 2: Specific branch (iac)**

```bash
az ad app federated-credential create \
  --id $CLIENT_ID \
  --parameters '{
    "name": "github-deploy-iac",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:'"$GITHUB_ORG"'/'"$GITHUB_REPO"':ref:refs/heads/iac",
    "audiences": ["api://AzureADTokenExchange"]
  }'
```

**Option 3: All pull requests (allows deployments from any PR)**

```bash
az ad app federated-credential create \
  --id $CLIENT_ID \
  --parameters '{
    "name": "github-deploy-pr",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:'"$GITHUB_ORG"'/'"$GITHUB_REPO"':pull_request",
    "audiences": ["api://AzureADTokenExchange"]
  }'
```

**Option 4: Environment-based (recommended - allows any branch to deploy to specific environment)**

```bash
# For staging environment
az ad app federated-credential create \
  --id $CLIENT_ID \
  --parameters '{
    "name": "github-deploy-staging",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:'"$GITHUB_ORG"'/'"$GITHUB_REPO"':environment:staging",
    "audiences": ["api://AzureADTokenExchange"]
  }'

# For production environment
az ad app federated-credential create \
  --id $CLIENT_ID \
  --parameters '{
    "name": "github-deploy-production",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:'"$GITHUB_ORG"'/'"$GITHUB_REPO"':environment:production",
    "audiences": ["api://AzureADTokenExchange"]
  }'
```

**Recommendation**: Use Option 4 (environment-based) for maximum flexibility. This allows you to deploy from any branch while controlling access through GitHub environment protection rules.

**Note**: You can create multiple federated credentials for the same service principal.

```bash
az ad app federated-credential create \
  --id $CLIENT_ID \
  --parameters '{
    "name": "github-deploy-pr",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:'"$GITHUB_ORG"'/'"$GITHUB_REPO"':pull_request",
    "audiences": ["api://AzureADTokenExchange"]
  }'
```

### 1.6 Assign Additional Permissions (if needed)

If your service principal needs to read Azure AD for SQL admin setup:

```bash
# Get object ID of the service principal
OBJECT_ID=$(az ad sp list --display-name "$SP_NAME" --query "[0].id" -o tsv)

# Assign Directory Readers role (requires Global Administrator)
# This allows the SP to read Azure AD users/groups for SQL admin configuration
az rest --method POST \
  --uri "https://graph.microsoft.com/v1.0/directoryRoles/roleTemplateId=88d8e3e3-8f55-4a1e-953a-9b9898b8876b/members/\$ref" \
  --body "{'@odata.id': 'https://graph.microsoft.com/v1.0/directoryObjects/$OBJECT_ID'}"
```

## Step 2: Configure GitHub Secrets and Variables

Navigate to your GitHub repository: **Settings → Secrets and variables → Actions**

### 2.1 Required Secrets

Create the following **Repository secrets**:

| Secret Name             | Description                               | How to Get                                     |
| ----------------------- | ----------------------------------------- | ---------------------------------------------- |
| `AZURE_CLIENT_ID`       | Service principal application (client) ID | From Step 1.4 output                           |
| `AZURE_TENANT_ID`       | Azure AD tenant ID                        | Run: `az account show --query tenantId -o tsv` |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription ID                     | Run: `az account show --query id -o tsv`       |

### 2.2 Required Variables

Create the following **Repository variables**:

| Variable Name             | Description                          | Example Value                                         |
| ------------------------- | ------------------------------------ | ----------------------------------------------------- |
| `AZURE_RESOURCE_GROUP`    | Resource group name for deployment   | `brleight-ground-truth-app-rg`                        |
| `AZURE_LOCATION`          | Azure region for resources           | `centralus`                                           |
| `RESOURCE_NAME_PREFIX`    | Prefix for all Azure resources       | `brleight`                                            |
| `SQL_AAD_ADMIN_LOGIN`     | Azure AD admin email for SQL servers | `admin@contoso.com`                                   |
| `SQL_AAD_ADMIN_OBJECT_ID` | Azure AD admin object ID             | Run: `az ad user show --id <email> --query id -o tsv` |

### 2.3 Optional Variables

| Variable Name                  | Description                                                      | Default |
| ------------------------------ | ---------------------------------------------------------------- | ------- |
| `SQL_AAD_ADMIN_PRINCIPAL_TYPE` | Type of SQL admin (User/Group/ServicePrincipal)                  | `User`  |
| `ADMIN_CLIENT_IP_ADDRESS`      | Your IP for SQL firewall (leave empty for private endpoint only) | ``      |
| `APP_SERVICE_PLAN_SKU`         | App Service Plan SKU                                             | `B1`    |

### 2.4 Getting Azure AD Admin Object ID

```bash
# For a user
az ad user show --id "admin@contoso.com" --query id -o tsv

# For a group
az ad group show --group "SQL Admins" --query id -o tsv

# For a service principal
az ad sp show --id <client-id> --query id -o tsv
```

## Step 3: Verify Configuration

### 3.1 Test Azure Authentication

Create a test workflow or use the infrastructure deployment workflow to verify authentication works.

### 3.2 Verify Permissions

Ensure your service principal can:

- Create/update resource groups
- Deploy Bicep templates
- Read Azure AD (if using user/group for SQL admin)

Test with:

```bash
# Login as service principal (testing only)
az login --service-principal \
  -u $CLIENT_ID \
  -p <secret> \
  --tenant $TENANT_ID

# Test resource group creation
az group create --name test-rg --location centralus

# Clean up
az group delete --name test-rg --yes
```

## Step 4: Configure Environments (Optional)

For production deployments with approval gates:

1. Go to **Settings → Environments**
2. Click **New environment**
3. Name it `production` or `staging`
4. Add **Protection rules**:
   - Required reviewers
   - Wait timer
   - Deployment branches

## Step 5: Enable Workflows

1. Commit and push the workflow files to your repository
2. Go to **Actions** tab in GitHub
3. Workflows should appear automatically
4. You can manually trigger workflows using the **Run workflow** button

## Workflow Triggers

| Workflow       | Automatic Trigger     | Manual Trigger      |
| -------------- | --------------------- | ------------------- |
| Infrastructure | Push to `infra/**`    | ✅ Yes               |
| Backend        | Push to `backend/**`  | ✅ Yes               |
| Frontend       | Push to `frontend/**` | ✅ Yes               |
| Seed Data      | ❌ No                  | ✅ Yes (manual only) |

## Troubleshooting

### Authentication Failures

**Error**: "AADSTS70021: No matching federated identity record found"

**Solution**: Verify federated credential subject matches your branch/PR pattern exactly.

```bash
# List federated credentials
az ad app federated-credential list --id $CLIENT_ID
```

### Permission Errors

**Error**: "AuthorizationFailed: The client does not have authorization to perform action"

**Solution**: Ensure service principal has Contributor role on subscription or resource group.

```bash
# Check role assignments
az role assignment list --assignee $CLIENT_ID --output table
```

### Bicep Deployment Failures

**Error**: "InvalidTemplateDeployment"

**Solution**: Test Bicep deployment locally first:

```bash
cd infra/deploy
az deployment group create \
  --resource-group <rg-name> \
  --template-file main.bicep \
  --parameters @main.parameters.json \
  --parameters resourceNamePrefix=<prefix> \
  --what-if
```

## Security Best Practices

1. **Never commit secrets**: Always use GitHub Secrets, never hardcode credentials
2. **Principle of least privilege**: Grant minimum necessary permissions to service principal
3. **Use federated credentials**: Avoid long-lived secrets (clientSecret)
4. **Enable branch protection**: Require PR reviews for main branch
5. **Use environments**: Add approval gates for production deployments
6. **Rotate credentials**: Regularly rotate service principal credentials
7. **Audit access**: Review service principal permissions quarterly

## Multi-Environment Setup

To add a production environment:

1. **Create separate resource group**: `<prefix>-ground-truth-app-prod-rg`
2. **Create environment in GitHub**: Settings → Environments → New environment
3. **Add environment-specific secrets/variables**: Scope secrets to specific environments
4. **Update workflows**: Add environment parameter to workflow inputs
5. **Configure approval**: Add required reviewers for production environment

Example workflow modification:

```yaml
jobs:
  deploy:
    runs-on: ubuntu-latest
    environment: ${{ github.event.inputs.environment || 'staging' }}
    # ... rest of job
```

## Additional Resources

- [Azure Federated Credentials Documentation](https://learn.microsoft.com/en-us/azure/developer/github/connect-from-azure)
- [GitHub Actions Environments](https://docs.github.com/en/actions/deployment/targeting-different-environments)
- [App Service Deployment Documentation](https://learn.microsoft.com/en-us/azure/app-service/deploy-github-actions)
- [Bicep Deployment with GitHub Actions](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/deploy-github-actions)

## Next Steps

1. Complete all setup steps above
2. Review and customize workflow files in `.github/workflows/`
3. Test each workflow with manual triggers first
4. Configure branch protection rules
5. Document your specific deployment process for your team
