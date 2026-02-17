# -----------------------------
# Stage 1: Build
# -----------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

# Copy the API csproj first
COPY src/trx-aggregation-api/*.csproj ./trx-aggregation-api/

# Restore dependencies
RUN dotnet restore ./trx-aggregation-api/trx-aggregation-api.csproj

# Copy all API source files
COPY src/trx-aggregation-api/ ./trx-aggregation-api/

# Build and publish the API project
RUN dotnet publish ./trx-aggregation-api/trx-aggregation-api.csproj -c Release -o /app/publish

# -----------------------------
# Stage 2: Runtime
# -----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

# Copy the published output
COPY --from=build /app/publish .

EXPOSE 5000

ENTRYPOINT ["dotnet", "trx-aggregation-api.dll"]
