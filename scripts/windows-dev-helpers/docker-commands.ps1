# Build all services
docker-compose build

# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop all services
docker-compose down

# Remove all volumes (clean slate)
docker-compose down -v

# Rebuild a specific service
docker-compose build sysmlstore-api
docker-compose up -d sysmlstore-api
