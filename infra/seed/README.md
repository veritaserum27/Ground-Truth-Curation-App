# Database Seeding Guide

This directory contains scripts and data for seeding all databases in the Ground Truth Curation App.

## 📁 Directory Structure

```text
infra/seed/
├── README.md                    # This file
├── .env.example                 # Environment variable template
├── requirements.txt             # Python dependencies
├── setup.sh                     # Virtual environment setup script
├── seed-all.sh                  # Master orchestration script
│
├── groundtruth/                 # Ground Truth SQL Database seeding
│   ├── prepare-gt-db.py         # Automated schema setup
│   ├── sql_utils.py             # Shared SQL utilities
│   └── sql/                     # SQL schema scripts (run in order)
│       ├── 01-create-ground-truth-tables.sql
│       ├── 02-create-ground-truth-tag-relationships.sql
│       ├── 03-create-conversation-tables.sql
│       └── 04-insert-sample-ground-truth-data.sql (optional)
│
├── support-tickets/             # System SQL Database seeding
│   ├── import-support-tickets.py    # CSV import script
│   ├── sql/
│   │   └── create-support-tickets-table.sql
│   └── data/
│       └── Support_tickets.zip      # 48,900 support ticket records
│
└── cosmos/                      # Cosmos DB seeding
    ├── upload-defects.py        # CSV upload script
    └── data/
        └── defects_data_with_company.zip  # 1,000 defect records
```

## 🎯 Purpose & Data Distribution

### Ground Truth SQL Database
- **Server**: `ground-truth-curation-sql`
- **Database**: `GroundTruthDB`
- **Purpose**: Stores curation workflow, ground truth definitions, tags, and comments
- **Authentication**: Azure AD (passwordless via DefaultAzureCredential)
- **Tables Created**: 10 tables including `GROUND_TRUTH_DEFINITION`, `GROUND_TRUTH_ENTRY`, `DATA_QUERY_DEFINITION`, `TAG`, `COMMENT`, `CONVERSATION`, etc.

### System SQL Database
- **Server**: `gt-system-data-sql`
- **Database**: `SystemDemoDB` or `ManufacturingDataRelDB`
- **Purpose**: Stores demo/system data that AI queries (e.g., support tickets)
- **Authentication**: SQL authentication (username/password)
- **Tables Created**: `support_tickets` (48,900 records)

### Cosmos DB
- **Account**: Your Cosmos DB account name
- **Database**: `ManufacturingDefects` or `ManufacturingDataDocDB`
- **Container**: `repairs` (partitioned by `/partitionKey`)
- **Purpose**: NoSQL demo data (manufacturing defects/repairs)
- **Authentication**: Azure AD (falls back to access keys)
- **Documents**: 1,000 manufacturing defect/repair records

## 🚀 Quick Start

### 1. Prerequisites

- **Azure Resources Deployed**: SQL servers and Cosmos DB account must exist (see the [deployment instructions](../README.md))
- **Python 3.8+**: Required for running seed scripts
- **Azure CLI**: For Azure AD authentication (`az login`)
- **ODBC Driver 18 for SQL Server**: For SQL Server connectivity

### 2. Setup Virtual Environment

```bash
cd infra/seed
./setup.sh
```

This will:
- Create a Python virtual environment at `.venv`
- Install all required dependencies from `requirements.txt`

### 3. Configure Environment Variables

If you executed [../deploy/deploy.sh](../deploy/deploy.sh), the values required are populated in your `.env` in that directory.

```bash
# Copy the example file
cp .env.example .env

# Edit .env with your actual values
nano .env  # or use your preferred editor
```

Fill in the required values:
- `GT_AZURE_SQL_CONNECTIONSTRING` - Ground Truth SQL connection string
- `SYSTEM_DB_SERVER`, `SYSTEM_DB_USERNAME`, `SYSTEM_DB_PASSWORD` - System SQL credentials
- `COSMOS_ENDPOINT`, `COSMOS_PRIMARY_KEY` - Cosmos DB credentials

### 4. Authenticate with Azure

```bash
# Login to Azure (for Azure AD authentication)
az login

# Verify you're using the correct subscription
az account show
```

### 5. Run Master Orchestration Script

```bash
# Activate virtual environment
source .venv/bin/activate

# Seed all databases (without sample data)
./seed-all.sh

# OR: Seed all databases including sample ground truth data
./seed-all.sh --seed-sample-data
```

The script will:
1. Seed Ground Truth SQL Database (schema creation)
2. Seed System SQL Database (import support tickets CSV)
3. Seed Cosmos DB (upload manufacturing defects CSV)

## 🔧 Manual Seeding (Individual Scripts)

If you prefer to run scripts individually or troubleshoot specific steps:

### Ground Truth SQL Database

```bash
source .venv/bin/activate

# Basic schema setup (creates tables, tags)
python groundtruth/prepare-gt-db.py --verbose

# Include sample ground truth data (dev/test only)
python groundtruth/prepare-gt-db.py --verbose --seed
```

**What it does**:
- Executes SQL scripts in order: `01-*.sql`, `02-*.sql`, `03-*.sql`
- Optionally executes `04-insert-sample-ground-truth-data.sql` with `--seed` flag
- Uses Azure AD passwordless authentication

### System SQL Database

```bash
source .venv/bin/activate

# Import support tickets CSV (auto-extracts from zip)
python support-tickets/import-support-tickets.py
```

**What it does**:
- Auto-extracts `Support_tickets.zip` if needed
- Creates `support_tickets` table if it doesn't exist
- Imports 48,900 records in chunks of 1000
- Uses MERGE statement to handle duplicates
- Refreshes connection every 500 records to prevent timeouts

### Cosmos DB

```bash
source .venv/bin/activate

# Upload manufacturing defects CSV (auto-extracts from zip)
python cosmos/upload-defects.py
```

**What it does**:
- Auto-extracts `defects_data_with_company.zip` if needed
- Creates database `ManufacturingDefects` and container `repairs` if needed
- Uploads 1,000 documents
- Tries Azure AD authentication first, falls back to access keys

## 🔄 Migration from Old Structure

If you have existing scripts referencing the old folder structure:

| Old Path                                                   | New Path                                                                  |
| ---------------------------------------------------------- | ------------------------------------------------------------------------- |
| `infra/seed/scripts/import-support-tickets-csv.py`         | `infra/seed/support-tickets/import-support-tickets.py`                    |
| `infra/seed/scripts/upload-defects-csv.py`                 | `infra/seed/cosmos/upload-defects.py`                                     |
| `infra/seed/sql/prepare-gt-db.py`                          | `infra/seed/groundtruth/prepare-gt-db.py`                                 |
| `infra/seed/sql/sql_utils.py`                              | `infra/seed/groundtruth/sql_utils.py`                                     |
| `infra/seed/sql/create-ground-truth-tables.sql`            | `infra/seed/groundtruth/sql/01-create-ground-truth-tables.sql`            |
| `infra/seed/sql/create-ground-truth-tag-relationships.sql` | `infra/seed/groundtruth/sql/02-create-ground-truth-tag-relationships.sql` |
| `infra/seed/sql/create-conversation-tables.sql`            | `infra/seed/groundtruth/sql/03-create-conversation-tables.sql`            |
| `infra/seed/sql/insert-sample-ground-truth-data.sql`       | `infra/seed/groundtruth/sql/04-insert-sample-ground-truth-data.sql`       |
| `infra/seed/sql/create-support-tickets-table.sql`          | `infra/seed/support-tickets/sql/create-support-tickets-table.sql`         |
| `infra/seed/data/Support_tickets.zip`                      | `infra/seed/support-tickets/data/Support_tickets.zip`                     |
| `infra/seed/data/defects_data_with_company.zip`            | `infra/seed/cosmos/data/defects_data_with_company.zip`                    |
| `infra/seed/scripts/requirements.txt`                      | `infra/seed/requirements.txt` (consolidated)                              |
| `infra/seed/sql/requirements.txt`                          | `infra/seed/requirements.txt` (consolidated)                              |
| `infra/seed/scripts/setup.sh`                              | `infra/seed/setup.sh` (updated)                                           |

**Environment Variable Changes**:
- `DB_SERVER` → `SYSTEM_DB_SERVER`
- `DB_DATABASE` → `SYSTEM_DB_DATABASE`
- `DB_USERNAME` → `SYSTEM_DB_USERNAME`
- `DB_PASSWORD` → `SYSTEM_DB_PASSWORD`
- `DB_DRIVER` → `SYSTEM_DB_DRIVER`
- `DB_PORT` → `SYSTEM_DB_PORT`
- `AZURE_SQL_CONNECTIONSTRING` → `GT_AZURE_SQL_CONNECTIONSTRING`

## 🐛 Troubleshooting

### Connection Timeout Issues

**System SQL Database**:
- The import script automatically refreshes the connection every 500 records
- If you still encounter timeouts, reduce chunk size in `import-support-tickets.py` (line 317)

**Ground Truth SQL Database**:
- Ensure you've run `az login` before executing scripts
- Check your firewall rules allow your client IP
- Verify the connection string in `.env` is correct

### Missing ODBC Driver

If you see "Driver not found" errors:

**macOS**:

```bash
brew tap microsoft/mssql-release https://github.com/Microsoft/homebrew-mssql-release
brew update
brew install msodbcsql18
```

**Ubuntu/Debian**:

```bash
curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add -
curl https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/prod.list > /etc/apt/sources.list.d/mssql-release.list
sudo apt-get update
sudo ACCEPT_EULA=Y apt-get install -y msodbcsql18
```

### Azure AD Authentication Failed

Ensure you're logged in to Azure CLI:

```bash
az login
az account show
```

If using a service principal or managed identity, ensure it has appropriate permissions:
- Ground Truth SQL: `db_owner` or `db_ddladmin` + `db_datawriter`
- System SQL: `db_datawriter`
- Cosmos DB: `Cosmos DB Data Contributor` role

### CSV Files Not Found

The scripts automatically extract ZIP files. If you see "CSV file not found":

```bash
# Manually extract data files
cd infra/seed/system/data
unzip Support_tickets.zip

cd ../../cosmos/data
unzip defects_data_with_company.zip
```

## 📚 Additional Resources

- **Azure SQL Quickstart**: [Python + Azure SQL Database](https://learn.microsoft.com/azure/azure-sql/database/azure-sql-python-quickstart)
- **Cosmos DB Python SDK**: [Azure Cosmos DB Python SDK](https://learn.microsoft.com/azure/cosmos-db/nosql/sdk-python)
- **Infrastructure Deployment**: See `infra/deploy/README.md` for deploying Azure resources
- **Database Schema Documentation**: See `infra/DATABASE_SCRIPTS_README.md` for table relationships

## 🔒 Security Notes

- **Never commit `.env` file**: It contains sensitive credentials
- **Use Azure AD when possible**: Passwordless authentication is more secure
- **Rotate credentials regularly**: Especially SQL authentication passwords
- **Limit IP access**: Configure SQL Server firewall rules to allow only trusted IPs
- **Use private endpoints**: For production, use private endpoints instead of public access

## 📝 Notes

- **Sample data is optional**: The `--seed-sample-data` flag is for dev/test environments only
- **Idempotent scripts**: All scripts can be run multiple times safely (uses MERGE/upsert logic)
- **Data file size**: Support tickets CSV is ~15MB, defects CSV is ~300KB
- **Execution time**: Complete seeding takes ~5-10 minutes depending on network speed
