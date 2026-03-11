# -----------------------------
# Stage 1: Build
# -----------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# 1. Copy the entire source directory into the container
# This might include your local Windows bin/obj folders
COPY src/trx-aggregation-api/ ./trx-aggregation-api/

# 2. CRITICAL: Delete any local Windows-based build artifacts 
# that were just copied, ensuring a clean slate for Linux.
RUN find ./trx-aggregation-api -type d -name "obj" -exec rm -rf {} +
RUN find ./trx-aggregation-api -type d -name "bin" -exec rm -rf {} +

# 3. Restore dependencies inside the Linux environment
# This generates the correct Linux project.assets.json
RUN dotnet restore ./trx-aggregation-api/trx-aggregation-api.csproj

# 4. Publish the app (using the fresh Linux assets)
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

# Copy the published output from the build stage
COPY --from=build /app/publish .

# Map internal app port
EXPOSE 8080

ENTRYPOINT ["dotnet", "trx-aggregation-api.dll"]