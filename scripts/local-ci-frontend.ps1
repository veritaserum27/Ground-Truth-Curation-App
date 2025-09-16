#!/usr/bin/env pwsh
#
# Local CI emulation script for Frontend
# Mimics the GitHub Actions ci-frontend.yml workflow
#
# Prerequisites: Node.js 20+, pnpm 8+
# Usage: ./scripts/local-ci-frontend.ps1
#

param(
    [string]$Configuration = "development"  # or "production"
)

Write-Host "[CI-LOCAL] Frontend CI emulation started" -ForegroundColor Green
Write-Host "[CI-LOCAL] Using configuration: $Configuration" -ForegroundColor Yellow

# Ensure we are at repo root (script is in scripts/)
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot  = Resolve-Path (Join-Path $scriptDir '..')
Set-Location $repoRoot

if (-not (Test-Path 'frontend/package.json')) {
  Write-Error "Cannot find frontend/package.json. Run from repo root or adjust paths."; exit 1
}

# Check prerequisites
Write-Host "[CI-LOCAL] node --version" -ForegroundColor Yellow
node --version
if ($LASTEXITCODE -ne 0) { Write-Error "Node.js not found. Install Node.js 20+"; exit 1 }

Write-Host "[CI-LOCAL] pnpm --version" -ForegroundColor Yellow
$pnpmVersion = & pnpm --version 2>$null
if ($LASTEXITCODE -ne 0) { 
  Write-Host "pnpm not found. Installing pnpm 8.x via npm..." -ForegroundColor Yellow
  npm install -g pnpm@8
  if ($LASTEXITCODE -ne 0) { Write-Error "Failed to install pnpm"; exit 1 }
} elseif ($pnpmVersion -and $pnpmVersion.StartsWith("10.")) {
  Write-Host "pnpm $pnpmVersion detected. This lockfile requires pnpm 8.x. Downgrading..." -ForegroundColor Yellow
  npm install -g pnpm@8
  if ($LASTEXITCODE -ne 0) { Write-Error "Failed to downgrade pnpm"; exit 1 }
}

# Verify final version
Write-Host "Using pnpm version: $(& pnpm --version)" -ForegroundColor Yellow

# Navigate to frontend directory
Set-Location frontend

Write-Host "[CI-LOCAL] Installing dependencies" -ForegroundColor Yellow
pnpm install --frozen-lockfile
if ($LASTEXITCODE -ne 0) { Write-Error "Dependencies installation failed"; exit 1 }

Write-Host "[CI-LOCAL] Skipping type check (TypeScript not configured)" -ForegroundColor Yellow

Write-Host "[CI-LOCAL] Building application" -ForegroundColor Yellow
$env:NODE_ENV = $Configuration
pnpm build
if ($LASTEXITCODE -ne 0) { Write-Error "Build failed"; exit 1 }

Write-Host "[CI-LOCAL] Build artifacts:" -ForegroundColor Yellow
if (Test-Path "build") {
  Get-ChildItem -Path "build" -Recurse | Select-Object Name, Length | Format-Table
} elseif (Test-Path "dist") {
  Get-ChildItem -Path "dist" -Recurse | Select-Object Name, Length | Format-Table
} else {
  Write-Warning "No build output directory found. Check build configuration."
}

# Return to repo root
Set-Location $repoRoot

Write-Host "[CI-LOCAL] Frontend CI completed successfully!" -ForegroundColor Green