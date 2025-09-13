# Ground Truth Curation App - Azure Infrastructure

This directory contains the Azure infrastructure as code (IaC) for the Ground Truth Curation App using Bicep templates.

## Architecture

The infrastructure creates:
- **Resource Group**: `ground-truth-app-rg`
- **System SQL Server**: `gt-system-data-sql` for system/demo data
- **System SQL Database**: `ManufacturingDataRelDB` for system demo data
- **Ground Truth SQL Server**: `ground-truth-curation-sql` for curation workflow
- **Ground Truth SQL Database**: `GroundTruthCurationDB` for curation workflow data
- **Azure Cosmos DB**: `gt-system-data-cosmos` serverless NoSQL database for system/demo data

## Files

- `main.bicep` - Main Bicep template defining all Azure resources
- `main.parameters.json` - Parameters file with required values only
- `create-support-tickets-table.sql` - SQL script to create the support_tickets table
- `create-ground-truth-tables.sql` - SQL script to create ground truth curation tables
- `create-ground-truth-tag-relationships.sql` - SQL script for tag relationships and default tags
- `insert-sample-ground-truth-data.sql` - SQL script with sample ground truth data
- `drop-ground-truth-tables.sql` - SQL script to clean up ground truth tables (development only)
- `DATABASE_SCRIPTS_README.md` - Documentation for all database scripts
- `import-support-tickets-csv.py` - Python script for importing CSV data to the database
- `upload-defects-csv.py` - Python script for uploading defects data to Cosmos DB
- `deploy.sh` - Automated deployment script with automatic existence checking
- `data/Support_tickets.csv` - Sample data for seeding the SQL database (48,900 records)
- `data/defects_data_with_company.csv` - Manufacturing defects data for Cosmos DB

## Deployment Prerequisites

1. **Azure CLI** - Install from [https://docs.microsoft.com/en-us/cli/azure/install-azure-cli](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
2. **Azure Subscription** - You'll need an active Azure subscription
3. **Permissions** - Contributor access to create resources
4. **Python 3.7+** - For running the CSV import script

   **Option A: Use the automated setup script (Recommended)**

   ```bash
   # Run the setup script (creates virtual environment and installs dependencies)
   ./setup.sh
   
   # After the script completes, activate the virtual environment
   cd .. && source .venv/bin/activate
   
   # Verify activation (you should see (.venv) in your prompt)
   which python
   ```

   **Option B: Manual setup**

   ```bash
   # Create virtual environment in project root
   cd .. && python3 -m venv .venv
   
   # Activate virtual environment
   source .venv/bin/activate
   
   # Install dependencies
   pip install -r infra/requirements.txt
   ```

## Quick Deployment

### Prerequisites

1. **Azure CLI** - Install from [https://docs.microsoft.com/en-us/cli/azure/install-azure-cli](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
2. **Azure Subscription** - You'll need an active Azure subscription
3. **Permissions** - Contributor access to create resources
4. **Environment File** - Create `.env` file with required password

### Setup Environment

1. **Create .env file**:

   ```bash
   cd infra
   cp .env.example .env
   # Edit .env and set SQL_ADMIN_PASSWORD=YourSecurePassword123!
   ```

2. **Login to Azure**:

   ```bash
   az login
   ```

### Automatic Resource Management

The deployment uses **automatic existence checking** - no manual configuration needed:
- ✅ **Deterministic naming**: All resources have predictable names
- ✅ **Incremental deployment**: Azure automatically skips existing resources
- ✅ **Idempotent**: Safe to run multiple times
- ✅ **No boolean flags**: No risk of human error with conditional parameters

### Option 1: Using the Deploy Script (Recommended)

1. **Configure parameters** (optional):

   ```bash
   # Edit main.parameters.json to customize settings
   nano main.parameters.json
   ```

2. **Run the deployment script**:

   ```bash
   cd infra
   ./deploy.sh
   ```

### Option 2: Manual Deployment

1. **Login to Azure**:

   ```bash
   az login
   ```

2. **Create Resource Group**:

   ```bash
   az group create --name ground-truth-app-rg --location westus2
   ```

3. **Deploy Infrastructure**:

   ```bash
   az deployment group create \
     --resource-group ground-truth-app-rg \
     --template-file main.bicep \
     --parameters @main.parameters.json
   ```

## Post-Deployment Steps

### 1. Setup System Data (Required)

1. **Create the support_tickets table**:
   - Connect to your **System SQL Database** (`ManufacturingDataRelDB`) using SQL Server Management Studio, Azure Data Studio, or the Azure portal
   - Server: `gt-system-data-sql.database.windows.net`
   - Run the SQL script: `create-support-tickets-table.sql`

   ![Screenshot of selecting SystemDemoDB database](./assets/SelectDbVsCodeSqlExtension.jpg)

2. **Import CSV data**:
    For Python CSV import tools (optional, only needed for database setup):

    ```sh
    # Run the setup script to create Python virtual environment
    ./setup.sh

    # Or manually:
    python3 -m venv .venv
    source .venv/bin/activate
    pip install -r infra/requirements.txt
    ```

    Import data:
   - Use the provided Python script: `python import-support-tickets-csv.py`
   - The script handles chunked imports to avoid connection timeouts
   - Successfully imports all 48,900 records from the CSV file

### 2. Setup Ground Truth Curation Database (Required)

1. **Create ground truth tables**:
   - Connect to your **Ground Truth SQL Database** (`GroundTruthDB`) using SQL Server Management Studio, Azure Data Studio, or the Azure portal
   - Server: `ground-truth-curation-sql.database.windows.net`
   - Run the SQL scripts in this order:
     1. `create-ground-truth-tables.sql` - Creates all main tables and relationships
     2. `create-ground-truth-tag-relationships.sql` - Creates tag relationships and default tags

2. **Load sample data** (Optional for development/testing):
   - Run the SQL script: `insert-sample-ground-truth-data.sql`
   - This provides realistic test data for all ground truth tables
   - **Skip this step in production environments**

3. **Verify setup**:
   - Check that all 10 ground truth tables were created successfully
   - Confirm default tags are present
   - Review sample data (if loaded) to understand the data model

   See `DATABASE_SCRIPTS_README.md` for detailed documentation of all tables and relationships.

## Configuration

### Database Settings

The default configuration creates a Basic tier SQL Database suitable for development and hackathons. The configuration includes:

- **Public Network Access**: Enabled for hackathon/development use
- **Mixed Authentication**: Both SQL Server auth and Azure AD supported
- **Open Firewall Rules**: Allows connections from any IP (development only)

### Security Settings

⚠️ **Important Security Notes**:

1. **Firewall rules allow all IPs** - This is configured for hackathon/development use
2. **Change the default admin password** in `main.parameters.json`
3. **Restrict firewall rules** for production deployments
4. **Mixed authentication** is enabled - disable Azure AD-only restriction for SQL auth

### Connection Information

After deployment, you'll get:
- **System SQL Server**: `gt-system-data-sql.database.windows.net`
- **System Database**: `ManufacturingDataRelDB` (for support tickets and system data)
- **Ground Truth SQL Server**: `ground-truth-curation-sql.database.windows.net`  
- **Ground Truth Database**: `GroundTruthCurationDB` (for curation workflow data)
- **Cosmos DB Account**: `gt-system-data-cosmos` (for manufacturing defects)
- Connection strings for both SQL databases
- Admin credentials (as configured in parameters)

## Support Tickets Table Schema

The table includes 33 columns optimized for the CSV data with proper data types:

| Column | Type | Description |
|--------|------|-------------|
| ticket_id | BIGINT | Primary key |
| day_of_week | NVARCHAR(10) | Day name |
| company_id | INT | Company identifier |
| priority | NVARCHAR(20) | Ticket priority |
| customer_sentiment | NVARCHAR(20) | Customer satisfaction (nullable) |
| error_rate_pct | DECIMAL(15,9) | Error rate percentage with high precision |
| product_area | NVARCHAR(50) | Product area affected |
| customer_tier | NVARCHAR(20) | Customer tier classification |
| region | NVARCHAR(50) | Geographic region |
| ... | ... | [See full schema in create-support-tickets-table.sql] |

### Key Features

- **DECIMAL(15,9)** for error_rate_pct to handle precise percentage values
- **Nullable customer_sentiment** to accommodate missing data
- **Optimized indexes** for common query patterns
- **Primary key** on ticket_id for data integrity

### Indexes

Optimized indexes are created for common query patterns:
- company_id
- priority  
- customer_tier
- product_area
- region
- day_of_week

## CSV Import Script

The `import-support-tickets-csv.py` script provides reliable data import with these features:

- **Chunked processing**: Imports data in 5,000-record batches to avoid timeouts
- **Error handling**: Robust connection management and retry logic
- **Progress tracking**: Shows import progress and statistics
- **Data validation**: Handles NULL values and data type conversions

### Usage

```bash
python import-support-tickets-csv.py
```

The script will prompt for:
- SQL Server name (e.g., ground-truth-sql-xyz.database.windows.net)
- Database name (SystemDemoDB)
- Username and password
- CSV file path

### Import Results

Successfully imports all 48,900 records from the Support_tickets.csv file.

## Troubleshooting

### Common Issues

1. **"Location not available"** - Change location in parameters file to `westus2`
2. **"Server name already exists"** - The script uses uniqueString() to avoid conflicts
3. **"Firewall blocking connection"** - Firewall rules are configured for open access in hackathon setup
4. **"Authentication failed"** - Use SQL Server authentication (not Azure AD) for import scripts

### Cleanup

To remove all resources:

```bash
az group delete --name ground-truth-app-rg --yes --no-wait
```

## Cost Estimation

Basic configuration costs approximately:
- SQL Database (Basic): ~$5/month
- SQL Server: No additional cost
- Storage: Included in Basic tier
- **Cosmos DB (Serverless)**: Pay-per-request pricing
  - No minimum charges or upfront costs
  - ~$0.25 per million Request Units (RUs) consumed
  - ~$0.25/GB per month for storage
  - Ideal for development, testing, and variable workloads

For production workloads, consider Standard or Premium tiers for SQL Database and evaluate provisioned throughput for Cosmos DB if you have predictable traffic patterns.

## Azure Cosmos DB Configuration

The infrastructure includes a serverless Azure Cosmos DB account with:

### Database and Containers
- **Database**: `ManufacturingDataDocDB`
- **Containers**:
  - `repairs`: Manufacturing defects and cost data (partitioned by `/partitionKey`)

### Cosmos DB Features
- **Serverless billing**: Only pay for consumed Request Units and storage
- **Automatic indexing**: All properties are indexed by default for flexible queries
- **Global accessibility**: NoSQL API compatible with MongoDB, SQL queries, and REST APIs
- **Hackathon-friendly**: Open public access for easy development
- **Deterministic naming**: `gt-system-data-cosmos` for predictable resource management

### Cosmos DB Connection Information
After deployment, you'll get:
- Cosmos DB Account Name: `gt-system-data-cosmos`
- Endpoint: `https://gt-system-data-cosmos.documents.azure.com:443/`
- Primary keys available through Azure portal or CLI

### Data Upload
Use the provided script to upload manufacturing defects data:

```bash
python upload-defects-csv.py
```

### Sample Commands

```bash
# Get Cosmos DB connection details
az cosmosdb show --name <cosmos-account-name> --resource-group ground-truth-app-rg
az cosmosdb keys list --name <cosmos-account-name> --resource-group ground-truth-app-rg
```

## Data Source

The infrastructure is designed to work with the following data sets [Kaggle.com](https://www.kaggle.com):

- [Support Tickets](https://www.kaggle.com/datasets/albertobircoci/support-ticket-priority-dataset-50k?resource=download)
- [Manufacturing Defects](https://www.kaggle.com/datasets/fahmidachowdhury/manufacturing-defects/data)

The Support Tickets dataset contains 48,900 records with detailed support ticket information including priorities, customer data, error rates, and regional information.

The Manufacturing Defects dataset has 1,000 records across 100 different product ids.

**NOTE:** For the purposes of Ground Truth Curation App development, I have modified the Manufacturing Defects data set to add a `company_id` that exists in the Support Tickets data set.

You will need to unzip [this dataset](./data/Support_tickets.zip) to open the support tickets `.csv`. Unzip [this dataset](./data/defects_data_with_company.csv.zip) to open the defects and cost data.