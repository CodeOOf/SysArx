# SysArx Quick Start Guide

Get SysArx running in minutes using the built-in Makefile automation.

## Prerequisites Check

Before starting, ensure you have:
- [ ] Docker Desktop installed and running
- [ ] Make utility installed (comes with Git Bash on Windows, or use `winget install GnuWin32.Make`)
- [ ] .NET 10.0 SDK (optional, only for local development)

> 💡 **Tip**: Run `make help` at any time to see all available commands

## Quick Start with Make

1. **Clone and navigate to the project:**
   ```bash
   git clone https://github.com/CodeOOf/SysArx.git
   cd SysArx
   ```

2. **See all available commands:**
   ```bash
   make help
   ```

3. **Start all services with Docker Compose:**
   ```bash
   make docker-up
   ```

4. **Wait for all services to start** (approximately 2-3 minutes)

5. **Access the applications:**
   - **Blazor Web UI**: http://localhost:5000
   - **Auth API (Swagger)**: http://localhost:5003/swagger
   - **SysMLStore API (Swagger)**: http://localhost:5001/swagger
   - **SysMLDiagram API (Swagger)**: http://localhost:5002/swagger
   - **Seq Logs**: http://localhost:5341
   - **RabbitMQ Management**: http://localhost:15672 (guest/guest)

## Common Make Commands

```bash
# Build and test locally (without Docker)
make build          # Build the solution
make test           # Run all tests
make test-security  # Run security tests only
make run-web        # Start the web application locally

# Docker commands
make docker         # Build all Docker images
make docker-up      # Start all services
make docker-down    # Stop all services
make docker-logs    # View logs from all services

# Development
make format         # Format code
make lint           # Check code formatting
make clean          # Clean build artifacts
make matrix         # Generate requirements matrix

# Full pipeline
make all            # Build, test, and generate reports
```

## Alternative: Manual Docker Compose

If you prefer not to use Make:

```bash
# Start all services
docker compose up -d

# View logs
docker compose logs -f

# Stop all services
docker compose down
```

## Test the Authentication

1. **Get test users list:**
   Visit: http://localhost:5003/api/auth/test-users

2. **Login via API:**
   ```bash
   # PowerShell
   $body = @{
       username = "admin"
       password = "admin123"
   } | ConvertTo-Json
   
   Invoke-RestMethod -Uri http://localhost:5003/api/auth/login -Method Post -Body $body -ContentType "application/json"
   ```

3. **Available test users:**
   - admin / admin123
   - user1 / user123
   - developer / dev123
   - architect / arch123

## Test SysMLStore API

1. **Create a SysML item:**
   ```bash
   # PowerShell
   $item = @{
       name = "Test Component"
       description = "A test SysML component"
       type = "Block"
       metadata = @{
           version = "1.0"
       }
       createdBy = "admin"
   } | ConvertTo-Json
   
   Invoke-RestMethod -Uri http://localhost:5001/api/sysmliTems -Method Post -Body $item -ContentType "application/json"
   ```

2. **Get all items:**
   ```bash
   Invoke-RestMethod -Uri http://localhost:5001/api/sysmliTems
   ```

## Test SysMLDiagram API

1. **Create a diagram:**
   ```bash
   # PowerShell
   $diagram = @{
       userId = "admin"
       diagramName = "My First Diagram"
       items = @()
   } | ConvertTo-Json
   
   Invoke-RestMethod -Uri http://localhost:5002/api/diagram -Method Post -Body $diagram -ContentType "application/json"
   ```

2. **Get diagram:**
   ```bash
   Invoke-RestMethod -Uri http://localhost:5002/api/diagram/admin
   ```

## Viewing Logs

**Seq Dashboard** (Recommended):
1. Open http://localhost:5341
2. View real-time logs from all services
3. Filter by service, log level, or search terms

**Docker Compose Logs**:
```bash
# Using Make
make docker-logs

# Or directly with Docker Compose
docker compose logs -f

# Specific service
docker compose logs -f web
```
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f sysmlstore-api
```

## Stopping the Application

```bash
# Stop all services
docker-compose down

# Stop and remove all data (clean slate)
docker-compose down -v
```

## Troubleshooting

### Service won't start
- Check if ports are available (5000-5003, 27017, 6379, 5672, 15672, 5341)
- View logs: `docker-compose logs [service-name]`

### MongoDB connection issues
- Ensure MongoDB container is running: `docker ps | grep mongodb`
- Connection string: `mongodb://admin:admin123@localhost:27017`

### Redis connection issues
- Ensure Redis container is running: `docker ps | grep redis`
- Dapr handles Redis connection automatically via statestore component

### LDAP issues
- For local dev, use test users (no actual LDAP connection needed)
- To use real LDAP: Update `LdapSettings:UseLocalDevelopmentMode` to `false`

## Next Steps

1. Explore the Swagger UIs for each API service
2. Check Seq logs at http://localhost:5341
3. Review the architecture in README.md
4. Start developing your Blazor UI components

## Development Mode

For local development without Docker:

1. **Start infrastructure services:**
   ```bash
   docker-compose up mongodb redis rabbitmq seq -d
   ```

2. **Run services locally with Dapr:**
   ```bash
   # Terminal 1 - SysMLStore API
   dapr run --app-id sysmlstore-api --app-port 5001 --dapr-http-port 3500 --components-path ./dapr/components -- dotnet run --project src/Services/SysMLStore

   # Terminal 2 - SysMLDiagram API
   dapr run --app-id sysmldiagram-api --app-port 5002 --dapr-http-port 3501 --components-path ./dapr/components -- dotnet run --project src/Services/SysMLDiagram

   # Terminal 3 - Auth API
   dotnet run --project src/Services/Auth

   # Terminal 4 - Blazor Web
   dotnet run --project src/web
   ```
