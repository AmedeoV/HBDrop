#!/bin/bash

# HBDrop Deployment Script
# This script is executed on the WSL2 server to deploy the application

set -e  # Exit on error

DEPLOY_PATH="${DEPLOY_PATH:-/home/$(whoami)/hbdrop}"
LOG_FILE="${DEPLOY_PATH}/deploy.log"

# Color codes for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

log() {
    echo -e "${GREEN}[$(date +'%Y-%m-%d %H:%M:%S')]${NC} $1" | tee -a "$LOG_FILE"
}

error() {
    echo -e "${RED}[$(date +'%Y-%m-%d %H:%M:%S')] ERROR:${NC} $1" | tee -a "$LOG_FILE"
    exit 1
}

warn() {
    echo -e "${YELLOW}[$(date +'%Y-%m-%d %H:%M:%S')] WARNING:${NC} $1" | tee -a "$LOG_FILE"
}

cd "$DEPLOY_PATH" || error "Failed to change to deployment directory"

log "Starting deployment..."

# Check if .env file exists
if [ ! -f .env ]; then
    warn ".env file not found. Creating from environment variables..."
    cat > .env << EOF
ENCRYPTION_MASTER_KEY=${ENCRYPTION_MASTER_KEY:-}
GIPHY_API_KEY=${GIPHY_API_KEY:-}
POSTGRES_PASSWORD=${POSTGRES_PASSWORD:-}
EOF
fi

# Stop existing containers
log "Stopping existing containers..."
docker-compose down || warn "Failed to stop containers (might not be running)"

# Backup baileys auth if it exists
log "Backing up WhatsApp auth data..."
if [ -d "HBDrop.Baileys/auth_info" ]; then
    cp -r HBDrop.Baileys/auth_info /tmp/baileys_auth_backup_$(date +%Y%m%d_%H%M%S) || warn "Failed to backup auth data"
fi

# Build new images
log "Building Docker images..."
docker-compose build --no-cache || error "Failed to build Docker images"

# Start containers
log "Starting containers..."
docker-compose up -d || error "Failed to start containers"

# Wait for containers to be healthy
log "Waiting for containers to be healthy..."
sleep 10

# Check container status
log "Checking container status..."
docker-compose ps

# Show recent logs
log "Recent application logs:"
docker-compose logs --tail=20 webapp

# Clean up
log "Cleaning up old Docker images..."
docker image prune -f || warn "Failed to prune images"

log "Deployment completed successfully!"
log "Application should be available at: https://hbdrop.step0fail.com"

exit 0
