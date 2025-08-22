#!/bin/bash

# Production Deployment Script for Yargısal Zeka Microservices
# Usage: ./deploy.sh [environment]

set -e

ENVIRONMENT=${1:-production}
COMPOSE_FILE="docker-compose.yml"

if [ "$ENVIRONMENT" = "production" ]; then
    COMPOSE_FILE="$COMPOSE_FILE -f docker-compose.prod.yml"
    echo "🚀 Deploying to PRODUCTION environment"
else
    echo "🧪 Deploying to DEVELOPMENT environment"
fi

# Check if .env file exists
if [ ! -f .env ]; then
    echo "❌ Error: .env file not found!"
    echo "📝 Please create .env file from .env.example template"
    exit 1
fi

# Validate required environment variables
REQUIRED_VARS=("DB_PASSWORD" "JWT_KEY" "GEMINI_API_KEY")
for var in "${REQUIRED_VARS[@]}"; do
    if [ -z "${!var}" ]; then
        echo "❌ Error: Required environment variable $var is not set!"
        exit 1
    fi
done

echo "🔍 Pulling latest images..."
docker-compose $COMPOSE_FILE pull

echo "🛑 Stopping existing containers..."
docker-compose $COMPOSE_FILE down

echo "🧹 Cleaning up unused resources..."
docker system prune -f

echo "🏗️ Building and starting services..."
docker-compose $COMPOSE_FILE up -d --build

echo "⏳ Waiting for services to be healthy..."
sleep 30

echo "🔍 Checking service health..."
docker-compose $COMPOSE_FILE ps

echo "✅ Deployment completed successfully!"
echo "🌐 Services should be available at:"
echo "  - API Gateway: http://localhost:5000"
echo "  - Frontend: http://localhost:80"
echo "  - Health Check: http://localhost:5000/health"

# Optional: Run health checks
echo "🏥 Running health checks..."
services=("api-gateway" "identity-service" "subscription-service" "search-service" "document-service" "ai-service")

for service in "${services[@]}"; do
    echo "Checking $service..."
    # Add your health check logic here
done

echo "🎉 All services are running!"
