<#
.SYNOPSIS
  Local reproduction of the .github/workflows/ci-backend.yml steps (build only, no tests currently).
.DESCRIPTION
  Mirrors the GitHub Actions workflow so you can validate it before committing.
  Performs: restore -> build (Release) -> docker build (no push).
.PARAMETER Configuration
  Build configuration (default: Release)
.NOTES
  Run from repo root:  pwsh ./scripts/local-ci-backend.ps1
#>
param(
  [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'

Write-Host "[CI-LOCAL] Using configuration: $Configuration" -ForegroundColor Cyan

# Ensure we are at repo root (script is in scripts/)
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot  = Resolve-Path (Join-Path $scriptDir '..')
Set-Location $repoRoot

if (-not (Test-Path 'backend/GroundTruthCuration.sln')) {
  Write-Error "Cannot find backend/GroundTruthCuration.sln. Run from repo root or adjust paths."; exit 1
}

# Derive short commit sha if git present
$sha = $(git rev-parse --short HEAD 2>$null) ; if (-not $sha) { $sha = (Get-Date -Format 'yyyyMMddHHmmss') }
$imageTag = "ground-truth-api:ci-$sha"

Write-Host "[CI-LOCAL] dotnet --version" -ForegroundColor Yellow
 dotnet --version

Write-Host "[CI-LOCAL] Restoring solution" -ForegroundColor Yellow
 dotnet restore backend/GroundTruthCuration.sln

Write-Host "[CI-LOCAL] Building solution" -ForegroundColor Yellow
 dotnet build backend/GroundTruthCuration.sln -c $Configuration --no-restore

Write-Host "[CI-LOCAL] Docker build (tag: $imageTag)" -ForegroundColor Yellow
 docker build -f backend/Dockerfile -t $imageTag backend

Write-Host "[CI-LOCAL] Listing newly built image" -ForegroundColor Yellow
 docker images --format "table {{.Repository}}\t{{.Tag}}\t{{.Size}}" | Select-String ground-truth-api | Out-String | Write-Host

Write-Host "[CI-LOCAL] Done." -ForegroundColor Green
