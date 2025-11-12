#!/bin/bash

# HBDrop Server Initial Setup Script
# Run this on your WSL2 server to prepare for deployments

set -e

# Colors
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${BLUE}╔════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║  HBDrop Server Setup Script           ║${NC}"
echo -e "${BLUE}╔════════════════════════════════════════╗${NC}"
echo ""

# Check if running on Linux
if [[ "$OSTYPE" != "linux-gnu"* ]]; then
    echo -e "${RED}This script must be run on Linux/WSL2${NC}"
    exit 1
fi

# Variables
DEPLOY_USER=${1:-$(whoami)}
DEPLOY_PATH="/home/$DEPLOY_USER/hbdrop"
DOMAIN="hbdrop.step0fail.com"

echo -e "${GREEN}Deployment user:${NC} $DEPLOY_USER"
echo -e "${GREEN}Deployment path:${NC} $DEPLOY_PATH"
echo -e "${GREEN}Domain:${NC} $DOMAIN"
echo ""

# Function to check if command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Update system
echo -e "${BLUE}[1/7] Updating system packages...${NC}"
sudo apt-get update
sudo apt-get upgrade -y

# Install Docker if not present
echo -e "${BLUE}[2/7] Installing Docker...${NC}"
if ! command_exists docker; then
    curl -fsSL https://get.docker.com -o get-docker.sh
    sudo sh get-docker.sh
    sudo usermod -aG docker $DEPLOY_USER
    rm get-docker.sh
    echo -e "${GREEN}✓ Docker installed${NC}"
else
    echo -e "${YELLOW}✓ Docker already installed${NC}"
fi

# Install Docker Compose Plugin
echo -e "${BLUE}[3/7] Installing Docker Compose...${NC}"
if ! docker compose version >/dev/null 2>&1; then
    sudo apt-get install -y docker-compose-plugin
    echo -e "${GREEN}✓ Docker Compose installed${NC}"
else
    echo -e "${YELLOW}✓ Docker Compose already installed${NC}"
fi

# Create deployment directory
echo -e "${BLUE}[4/7] Creating deployment directory...${NC}"
mkdir -p "$DEPLOY_PATH"
cd "$DEPLOY_PATH"
echo -e "${GREEN}✓ Directory created: $DEPLOY_PATH${NC}"

# Install Nginx
echo -e "${BLUE}[5/7] Installing Nginx...${NC}"
if ! command_exists nginx; then
    sudo apt-get install -y nginx
    echo -e "${GREEN}✓ Nginx installed${NC}"
else
    echo -e "${YELLOW}✓ Nginx already installed${NC}"
fi

# Configure Nginx
echo -e "${BLUE}[6/7] Configuring Nginx...${NC}"
sudo tee /etc/nginx/sites-available/hbdrop > /dev/null << 'EOF'
server {
    listen 80;
    server_name hbdrop.step0fail.com;

    location / {
        proxy_pass http://localhost:5007;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        
        # WebSocket support
        proxy_read_timeout 86400;
    }
}
EOF

sudo ln -sf /etc/nginx/sites-available/hbdrop /etc/nginx/sites-enabled/hbdrop
sudo nginx -t
sudo systemctl reload nginx
echo -e "${GREEN}✓ Nginx configured${NC}"

# Install Certbot for SSL
echo -e "${BLUE}[7/7] Installing Certbot for SSL...${NC}"
if ! command_exists certbot; then
    sudo apt-get install -y certbot python3-certbot-nginx
    echo -e "${GREEN}✓ Certbot installed${NC}"
else
    echo -e "${YELLOW}✓ Certbot already installed${NC}"
fi

echo ""
echo -e "${GREEN}╔════════════════════════════════════════╗${NC}"
echo -e "${GREEN}║  Setup Complete!                       ║${NC}"
echo -e "${GREEN}╚════════════════════════════════════════╝${NC}"
echo ""
echo -e "${YELLOW}Next Steps:${NC}"
echo ""
echo -e "1. ${BLUE}Configure GitHub Secrets:${NC}"
echo -e "   - SSH_PRIVATE_KEY: Your SSH private key"
echo -e "   - SSH_USER: $DEPLOY_USER"
echo -e "   - SSH_HOST: $(hostname -I | awk '{print $1}')"
echo -e "   - DEPLOY_PATH: $DEPLOY_PATH"
echo -e "   - ENCRYPTION_MASTER_KEY: $(openssl rand -base64 32)"
echo -e "   - GIPHY_API_KEY: Get from https://developers.giphy.com/"
echo ""
echo -e "2. ${BLUE}Set up SSL (after DNS is configured):${NC}"
echo -e "   sudo certbot --nginx -d $DOMAIN"
echo ""
echo -e "3. ${BLUE}Configure DNS:${NC}"
echo -e "   Point $DOMAIN to: $(hostname -I | awk '{print $1}')"
echo ""
echo -e "4. ${BLUE}Test SSH access:${NC}"
echo -e "   ssh $DEPLOY_USER@$(hostname -I | awk '{print $1}')"
echo ""
echo -e "5. ${BLUE}Push to GitHub:${NC}"
echo -e "   git push origin main"
echo ""
echo -e "${GREEN}The application will auto-deploy on push to main branch!${NC}"
echo ""

# Check if we need to logout/login for Docker group
if ! groups | grep -q docker; then
    echo -e "${YELLOW}⚠ You need to log out and back in for Docker permissions to take effect${NC}"
    echo -e "${YELLOW}  Or run: newgrp docker${NC}"
fi
