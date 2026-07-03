FROM mcr.microsoft.com/dotnet/sdk:8.0 AS agent-environment

WORKDIR /src

ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV DOTNET_NOLOGO=1
ENV ASPNETCORE_ENVIRONMENT=Testing

# Restore layer (tests + service dependencies only — no WebApi / Redis / RabbitMQ).
COPY JetFlight.sln ./
COPY tests/JetFlight.WebApiTests.csproj ./tests/
COPY JetFlight.Service/JetFlight.Service.csproj ./JetFlight.Service/
COPY JetFlight.Shared/JetFlight.Shared.csproj ./JetFlight.Shared/
COPY JetFlight.ApplicationDataAccess/JetFlight.ApplicationDataAccess.csproj ./JetFlight.ApplicationDataAccess/
COPY JetFlight.IntegrationDataAccess/JetFlight.IntegrationDataAccess.csproj ./JetFlight.IntegrationDataAccess/

RUN dotnet restore tests/JetFlight.WebApiTests.csproj

COPY tests/ ./tests/
COPY JetFlight.Service/ ./JetFlight.Service/
COPY JetFlight.Shared/ ./JetFlight.Shared/
COPY JetFlight.ApplicationDataAccess/ ./JetFlight.ApplicationDataAccess/
COPY JetFlight.IntegrationDataAccess/ ./JetFlight.IntegrationDataAccess/
COPY scripts/ ./scripts/

RUN chmod +x scripts/*.sh \
    && dotnet build tests/JetFlight.WebApiTests.csproj --configuration Release --no-restore

# All tests use in-memory SQLite (see TestIntegrationDataContext). No external DB required.
CMD ["./scripts/validate.sh"]
