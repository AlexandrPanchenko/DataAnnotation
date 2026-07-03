#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

echo "==> Building test project (in-memory SQLite, no external services)..."
dotnet build tests/JetFlight.WebApiTests.csproj --configuration Release

echo "==> Running environment smoke test..."
dotnet test tests/JetFlight.WebApiTests.csproj \
  --configuration Release \
  --no-build \
  --verbosity minimal \
  --filter "FullyQualifiedName~FlightLoyaltyServiceSavedPromotionTests.AddSavedPromotion_ReturnsFalse_WhenCustomerIdIsNull"

echo ""
echo "Environment validation passed."
echo "Task verification: docker compose --profile task run --rm task-tests"
