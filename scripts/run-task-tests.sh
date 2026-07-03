#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

dotnet build tests/JetFlight.WebApiTests.csproj --configuration Release

dotnet test tests/JetFlight.WebApiTests.csproj \
  --configuration Release \
  --no-build \
  --verbosity normal \
  --filter "FullyQualifiedName~JetFlight.WebApiTests.Services.FlightLoyalty"
