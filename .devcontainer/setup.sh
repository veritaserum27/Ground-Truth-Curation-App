#!/bin/bash
set -e

echo "🚀 Setting up Ground Truth Curation App..."

# Setup backend
echo "📦 Restoring .NET packages..."
cd backend
if ! dotnet restore GroundTruthCuration.sln; then
    echo "❌ Failed to restore .NET packages"
    exit 1
fi
echo "✅ .NET packages restored successfully"

# Setup frontend
echo "📦 Installing frontend dependencies..."
cd ../frontend
if ! pnpm install --frozen-lockfile; then
    echo "❌ Failed to install frontend dependencies"
    exit 1
fi
echo "✅ Frontend dependencies installed successfully"

echo "🎉 Setup completed successfully!"
echo "💡 Use 'dotnet run --project backend/src/GroundTruthCuration.Api' to start the backend"
echo "💡 Use 'cd frontend && pnpm dev' to start the frontend"