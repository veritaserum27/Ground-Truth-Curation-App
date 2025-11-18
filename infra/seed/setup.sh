#!/usr/bin/env bash
# ============================================================================
# Ground Truth Curation App - Python Environment Setup
# ============================================================================
# This script creates a Python virtual environment and installs dependencies
# for seeding databases (Ground Truth SQL, System SQL, Cosmos DB)
#
# Usage:
#   cd infra/seed
#   ./setup.sh

set -e  # Exit on error

echo "🐍 Setting up Python environment for seed scripts..."
echo "============================================================================"

# Check Python version
if ! command -v python3 &> /dev/null; then
    echo "❌ Python 3 is not installed. Please install Python 3.8 or higher."
    exit 1
fi

PYTHON_VERSION=$(python3 --version)
echo "✅ Found $PYTHON_VERSION"

# Ensure we're in the infra/seed directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

echo "📁 Working directory: $SCRIPT_DIR"

# Create virtual environment
VENV_DIR=".venv"

if [ -d "$VENV_DIR" ]; then
    echo "⚠️  Virtual environment already exists at $VENV_DIR"
    read -p "   Do you want to recreate it? (y/N): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        echo "🗑️  Removing existing virtual environment..."
        rm -rf "$VENV_DIR"
    else
        echo "ℹ️  Skipping virtual environment creation"
        exit 0
    fi
fi

echo "📦 Creating virtual environment at $VENV_DIR..."
python3 -m venv "$VENV_DIR"

echo "📥 Installing dependencies from requirements.txt..."
"$VENV_DIR/bin/pip" install --upgrade pip
"$VENV_DIR/bin/pip" install -r requirements.txt

echo ""
echo "============================================================================"
echo "✅ Setup complete!"
echo ""
echo "To activate the virtual environment, run:"
echo "   source $VENV_DIR/bin/activate"
echo ""
echo "To deactivate when done:"
echo "   deactivate"
echo ""
echo "Next steps:"
echo "   1. Copy .env.example to .env and fill in your database credentials"
echo "   2. Run seed-all.sh to seed all databases, or run individual scripts:"
echo "      - groundtruth/prepare-gt-db.py (Ground Truth SQL schema)"
echo "      - support-tickets/import-support-tickets.py (System SQL data)"
echo "      - cosmos/upload-defects.py (Cosmos DB data)"
echo "============================================================================"
