#!/usr/bin/env bash
# ============================================================================
# Ground Truth Curation App - Master Seed Orchestration Script
# ============================================================================
# This script orchestrates seeding of all databases in the correct order:
#   1. Ground Truth SQL Database (schema creation)
#   2. System SQL Database (demo support tickets data)
#   3. Cosmos DB (manufacturing defects NoSQL data)
#
# Prerequisites:
#   - Azure resources deployed (SQL servers, Cosmos DB account)
#   - Python virtual environment set up (run ./setup.sh first)
#   - .env file configured with connection details
#   - Azure CLI login (for Azure AD authentication): az login
#
# Usage:
#   cd infra/seed
#   ./seed-all.sh [--seed-sample-data]
#
# Options:
#   --seed-sample-data    Include sample ground truth data (dev/test only)

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Helper functions
print_header() {
    echo ""
    echo -e "${BLUE}============================================================================${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}============================================================================${NC}"
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

print_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

# Parse arguments
SEED_SAMPLE_DATA=false
while [[ $# -gt 0 ]]; do
    case $1 in
        --seed-sample-data)
            SEED_SAMPLE_DATA=true
            shift
            ;;
        *)
            print_error "Unknown option: $1"
            echo "Usage: ./seed-all.sh [--seed-sample-data]"
            exit 1
            ;;
    esac
done

# Get script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

print_header "Ground Truth Curation App - Database Seeding"
echo "This script will seed all databases in the correct order."
echo ""

# Check prerequisites
print_info "Checking prerequisites..."

# Check for .env file
if [ ! -f ".env" ]; then
    print_error ".env file not found!"
    echo ""
    echo "Please create a .env file with your database credentials."
    echo "You can copy .env.example and fill in the values:"
    echo "  cp .env.example .env"
    echo "  # Edit .env with your actual values"
    exit 1
fi
print_success ".env file found"

# Check for virtual environment
if [ ! -d ".venv" ]; then
    print_error "Virtual environment not found!"
    echo ""
    echo "Please run ./setup.sh first to create the virtual environment."
    exit 1
fi
print_success "Virtual environment found"

# Activate virtual environment
print_info "Activating virtual environment..."
source .venv/bin/activate

# Check Python is available
if ! command -v python &> /dev/null; then
    print_error "Python not found in virtual environment!"
    exit 1
fi
print_success "Python $(python --version) activated"

# Check required packages
print_info "Checking required packages..."
python -c "import pyodbc, azure.cosmos, azure.identity, dotenv" 2>/dev/null
if [ $? -ne 0 ]; then
    print_error "Required Python packages not installed!"
    echo ""
    echo "Please run ./setup.sh to install dependencies."
    exit 1
fi
print_success "All required packages installed"

# Confirm before proceeding
echo ""
print_warning "This will seed the following databases:"
echo "  1. Ground Truth SQL Database (schema + optional sample data)"
echo "  2. System SQL Database (support tickets CSV)"
echo "  3. Cosmos DB (manufacturing defects CSV)"
echo ""
if [ "$SEED_SAMPLE_DATA" = true ]; then
    print_warning "Sample ground truth data WILL be included (--seed-sample-data flag)"
else
    print_info "Sample ground truth data will NOT be included (use --seed-sample-data to include)"
fi
echo ""
read -p "Do you want to continue? (y/N): " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    print_info "Seeding cancelled by user"
    exit 0
fi

# Track overall success
OVERALL_SUCCESS=true

# ============================================================================
# Step 1: Seed Ground Truth SQL Database
# ============================================================================
print_header "Step 1/3: Seeding Ground Truth SQL Database"
echo "This will create the ground truth schema tables..."
echo ""

GT_ARGS=""
if [ "$SEED_SAMPLE_DATA" = true ]; then
    GT_ARGS="--seed"
    print_info "Sample data will be included"
fi

if python groundtruth/prepare-gt-db.py --verbose $GT_ARGS; then
    print_success "Ground Truth SQL Database seeded successfully"
else
    print_error "Ground Truth SQL Database seeding failed"
    OVERALL_SUCCESS=false
fi

# ============================================================================
# Step 2: Seed System SQL Database
# ============================================================================
print_header "Step 2/3: Seeding System SQL Database"
echo "This will import support tickets CSV data..."
echo ""

if python support-tickets/import-support-tickets.py; then
    print_success "System SQL Database seeded successfully"
else
    print_error "System SQL Database seeding failed"
    OVERALL_SUCCESS=false
fi

# ============================================================================
# Step 3: Seed Cosmos DB
# ============================================================================
print_header "Step 3/3: Seeding Cosmos DB"
echo "This will upload manufacturing defects CSV data..."
echo ""

if python cosmos/upload-defects.py; then
    print_success "Cosmos DB seeded successfully"
else
    print_error "Cosmos DB seeding failed"
    OVERALL_SUCCESS=false
fi

# ============================================================================
# Summary
# ============================================================================
print_header "Seeding Complete"

if [ "$OVERALL_SUCCESS" = true ]; then
    print_success "All databases seeded successfully! 🎉"
    echo ""
    echo "Next steps:"
    echo "  - Verify data in Azure Portal or Azure Data Studio"
    echo "  - Test your application with the seeded data"
    exit 0
else
    print_error "Some databases failed to seed. Please check the errors above."
    echo ""
    echo "You can run individual seed scripts to retry:"
    echo "  python groundtruth/prepare-gt-db.py --verbose [--seed]"
    echo "  python support-tickets/import-support-tickets.py"
    echo "  python cosmos/upload-defects.py"
    exit 1
fi
