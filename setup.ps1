#!/usr/bin/env pwsh
# Simple setup script - idempotent (can run multiple times)

Write-Host "🚀 Setting up infrastructure..." -ForegroundColor Green

# Run migrations (idempotent)
Write-Host "📊 Running migrations..." -ForegroundColor Yellow
try {
    dotnet ef database update --project HexagonalSkeleton.MigrationDb
    Write-Host "✅ Migrations completed" -ForegroundColor Green
} catch {
    Write-Host "❌ Migration failed: $_" -ForegroundColor Red
    exit 1
}

# Configure CDC (idempotent - delete if exists, then create)
Write-Host "🔗 Configuring CDC..." -ForegroundColor Yellow
try {
    # Delete existing connector (ignore errors)
    curl -X DELETE http://localhost:8083/connectors/postgres-users-connector 2>$null
    Start-Sleep 2
    
    # Create connector
    $response = curl -X POST -H "Content-Type: application/json" -d '{
        "name": "postgres-users-connector",
        "config": {
            "connector.class": "io.debezium.connector.postgresql.PostgresConnector",
            "tasks.max": "1",
            "database.hostname": "hexagonal-postgresql",
            "database.port": "5432",
            "database.user": "hexagonal_user",
            "database.password": "hexagonal_password",
            "database.dbname": "HexagonalSkeleton",
            "topic.prefix": "hexagonal-postgres",
            "table.include.list": "public.users",
            "plugin.name": "pgoutput",
            "publication.autocreate.mode": "filtered",
            "slot.name": "hexagonal_slot"
        }
    }' http://localhost:8083/connectors
    
    Write-Host "✅ CDC configured" -ForegroundColor Green
} catch {
    Write-Host "❌ CDC configuration failed: $_" -ForegroundColor Red
    exit 1
}

Write-Host "🎉 Setup completed! You can now run: dotnet run --project HexagonalSkeleton.API" -ForegroundColor Green
