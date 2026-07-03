#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

dotnet test tests/JetFlight.WebApiTests.csproj \
  --configuration Release \
  --no-build \
  --verbosity quiet \
  --filter "FullyQualifiedName~FlightLoyaltyServiceSavedPromotionTests.AddSavedPromotion_ReturnsFalse_WhenCustomerIdIsNull"

echo "ok"
