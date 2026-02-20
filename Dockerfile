# -----------------------------
# Stage 1: Build
# -----------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy csproj and restore dependencies
COPY src/trx-aggregation-api/*.csproj ./trx-aggregation-api/
RUN dotnet restore ./trx-aggregation-api/trx-aggregation-api.csproj

# Copy only source code (avoid bin/obj)
COPY src/trx-aggregation-api/ ./trx-aggregation-api/

# Publish the app (clean, Release)
RUN dotnet publish ./trx-aggregation-api/trx-aggregation-api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# -----------------------------
# Stage 2: Runtime
# -----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

# Copy the published output only
COPY --from=build /app/publish .

# Map internal app port (from launchSettings.json or appsettings.json)
EXPOSE 8080

ENTRYPOINT ["dotnet", "trx-aggregation-api.dll"]