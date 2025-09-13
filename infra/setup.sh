#!/bin/bash

# Setup script for Ground Truth Curation App - Python CSV Import Tools
# This script creates a virtual environment for the CSV import functionality
#
# USAGE: ./setup.sh
# NOTE: After running this script, you'll need to manually activate the virtual environment
#       with: source .venv/bin/activate

set -e  # Exit on any error

echo "🚀 Setting up Python environment for CSV import tools..."
echo "📝 This script will create a virtual environment and install dependencies."
echo "   After completion, you'll need to manually activate the environment."
echo ""

# Check if Python 3 is installed
if ! command -v python3 &> /dev/null; then
    echo "❌ Python 3 is not installed. Please install Python 3.7+ first."
    exit 1
fi

# Navigate to project root if we're in infra folder
if [[ "$(basename "$PWD")" == "infra" ]]; then
    cd ..
fi

# Create virtual environment if it doesn't exist
if [ ! -d ".venv" ]; then
    echo "📦 Creating virtual environment..."
    python3 -m venv .venv
    echo "✅ Virtual environment created"
else
    echo "📦 Virtual environment already exists"
fi

# Activate virtual environment
echo "🔌 Activating virtual environment..."
source .venv/bin/activate

# Upgrade pip
echo "⬆️  Upgrading pip..."
pip install --upgrade pip

# Install Python dependencies for CSV import
echo "📥 Installing Python dependencies for CSV import..."
pip install -r infra/requirements.txt

echo "✅ Python environment setup complete!"
echo ""
echo "🎯 IMPORTANT: The virtual environment was activated during setup, but you need to"
echo "   activate it manually in your current terminal session to use it."
echo ""
echo "To activate the virtual environment:"
echo "   cd .. (if you're in the infra directory)"
echo "   source .venv/bin/activate"
echo ""
echo "🛠️  To deactivate the virtual environment when you're done:"
echo "   deactivate"
echo ""
echo "📋 To run the CSV import script (after activating the virtual environment):"
echo "   cd infra && python import-support-tickets-csv.py"
echo ""
echo "💡 You'll know the virtual environment is active when you see (.venv) in your prompt."
