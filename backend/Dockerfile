# Multi-stage Dockerfile for Ground Truth Curation Backend (.NET 8)
# Optimized for Azure Container Apps & fast iterative builds.

# ---- Build Stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# ARG allows overriding configuration at build time: --build-arg BUILD_CONFIGURATION=Debug
ARG BUILD_CONFIGURATION=Release

# Expectation: build context is the backend folder (recommended). If building from repo root, use:
#   docker build -f backend/Dockerfile -t ground-truth-api:local backend
# so that the context still maps to this directory layout.

# Copy solution & project files first (improves layer caching)
COPY GroundTruthCuration.sln ./
COPY src/GroundTruthCuration.Core/GroundTruthCuration.Core.csproj src/GroundTruthCuration.Core/
COPY src/GroundTruthCuration.Infrastructure/GroundTruthCuration.Infrastructure.csproj src/GroundTruthCuration.Infrastructure/
COPY src/GroundTruthCuration.Api/GroundTruthCuration.Api.csproj src/GroundTruthCuration.Api/

# Restore dependencies
RUN dotnet restore GroundTruthCuration.sln

# Copy the remainder of the source
COPY . .

# Build solution (no restore)
RUN dotnet build GroundTruthCuration.sln -c ${BUILD_CONFIGURATION} --no-restore

# Publish API project (framework-dependent; trimming off for faster build-time)
RUN dotnet publish src/GroundTruthCuration.Api/GroundTruthCuration.Api.csproj -c ${BUILD_CONFIGURATION} -o /app/publish --no-build

# ---- Runtime Stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Set non-root user later if needed; Azure Container Apps runs as root by default unless configured.

# Environment (Azure Container Apps typically scans ingress on this port if specified)
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_EnableDiagnostics=0

# Copy published output
COPY --from=build /app/publish .

# (Optional) Health check endpoint assumption: /healthz (add later in API)
# HEALTHCHECK --interval=30s --timeout=5s --retries=3 CMD wget -qO- http://localhost:8080/healthz || exit 1

EXPOSE 8080

ENTRYPOINT ["dotnet", "GroundTruthCuration.Api.dll"]
