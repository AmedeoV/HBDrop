# HBDrop Deployment Guide

This guide explains how to set up automated deployment to your WSL2 server at `hbdrop.step0fail.com`.

## Overview

The deployment system uses GitHub Actions to automatically deploy the application whenever you push to the `main` branch. The workflow:

1. Packages the application code
2. Transfers it to your WSL2 server via SSH
3. Builds Docker images
4. Starts the containers with docker-compose

## Prerequisites

### On Your WSL2 Server

1. **Docker and Docker Compose installed**
   ```bash
   # Install Docker
   curl -fsSL https://get.docker.com -o get-docker.sh
   sudo sh get-docker.sh
   sudo usermod -aG docker $USER
   
   # Install Docker Compose
   sudo apt-get update
   sudo apt-get install docker-compose-plugin
   ```

2. **SSH access configured**
   - Your server should be accessible via SSH
   - Ensure your user has Docker permissions

3. **Create deployment directory**
   ```bash
   mkdir -p ~/hbdrop
   cd ~/hbdrop
   ```

### On Your Local Machine

1. **Generate SSH key pair (if you don't have one)**
   ```bash
   ssh-keygen -t ed25519 -C "github-actions-hbdrop" -f ~/.ssh/hbdrop_deploy
   ```

2. **Add public key to WSL2 server**
   ```bash
   ssh-copy-id -i ~/.ssh/hbdrop_deploy.pub your_user@your_server_ip
   ```

## GitHub Repository Setup

### Required Secrets

Go to your GitHub repository → Settings → Secrets and variables → Actions → New repository secret

Add the following secrets:

| Secret Name | Description | Example Value |
|-------------|-------------|---------------|
| `SSH_PRIVATE_KEY` | Private SSH key content | Contents of `~/.ssh/hbdrop_deploy` |
| `SSH_USER` | SSH username for WSL2 | `your_username` |
| `SSH_HOST` | Server IP or hostname | `192.168.x.x` or `your-server.local` |
| `DEPLOY_PATH` | Deployment directory path | `/home/your_username/hbdrop` |
| `ENCRYPTION_MASTER_KEY` | Encryption key for app | Generate with: `openssl rand -base64 32` |
| `GIPHY_API_KEY` | Giphy API key | Get from https://developers.giphy.com/ |
| `POSTGRES_PASSWORD` | PostgreSQL password | Generate secure password for production |

### Setting Up Secrets

1. **SSH_PRIVATE_KEY**:
   ```bash
   cat ~/.ssh/hbdrop_deploy
   ```
   Copy the entire output (including `-----BEGIN OPENSSH PRIVATE KEY-----` and `-----END OPENSSH PRIVATE KEY-----`)

2. **ENCRYPTION_MASTER_KEY**:
   ```bash
   openssl rand -base64 32
   ```

3. **GIPHY_API_KEY**:
   - Visit https://developers.giphy.com/
   - Create a free account
   - Create a new app to get an API key

4. **POSTGRES_PASSWORD**:
   ```bash
   # Generate a secure password
   openssl rand -base64 24
   ```
   Use a strong, unique password for production

## Nginx Configuration (Optional)

If you want to access the app at `hbdrop.step0fail.com`, set up Nginx as a reverse proxy:

### Install Nginx
```bash
sudo apt-get update
sudo apt-get install nginx
```

### Create Nginx Configuration
```bash
sudo nano /etc/nginx/sites-available/hbdrop
```

Add the following configuration:

```nginx
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
    }
}
```

### Enable the site
```bash
sudo ln -s /etc/nginx/sites-available/hbdrop /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

### SSL with Let's Encrypt (Recommended)
```bash
sudo apt-get install certbot python3-certbot-nginx
sudo certbot --nginx -d hbdrop.step0fail.com
```

## DNS Configuration

Point `hbdrop.step0fail.com` to your server's IP address:

1. Go to your DNS provider
2. Add an A record:
   - Name: `hbdrop`
   - Type: `A`
   - Value: `your_server_ip`
   - TTL: `3600`

## Manual Deployment

If you need to deploy manually without GitHub Actions:

```bash
# On your local machine
cd d:\Projects\HBDrop
tar czf deploy.tar.gz --exclude='.git' --exclude='node_modules' --exclude='bin' --exclude='obj' .

# Copy to server
scp deploy.tar.gz your_user@your_server:/home/your_user/hbdrop/

# SSH to server
ssh your_user@your_server

# Deploy
cd ~/hbdrop
tar xzf deploy.tar.gz
rm deploy.tar.gz

# Create .env file (first time only)
cat > .env << EOF
ENCRYPTION_MASTER_KEY=your_key_here
GIPHY_API_KEY=your_key_here
EOF

# Deploy
docker-compose down
docker-compose build --no-cache
docker-compose up -d
```

## Deployment Workflow

### Automatic Deployment

Simply push to the `main` branch:

```bash
git add .
git commit -m "Your changes"
git push origin main
```

GitHub Actions will automatically:
1. Build the application
2. Transfer files to your server
3. Build Docker images
4. Start containers

### Manual Trigger

You can also trigger deployment manually from GitHub:

1. Go to your repository on GitHub
2. Click "Actions" tab
3. Select "Deploy to Production" workflow
4. Click "Run workflow" button
5. Select the branch and click "Run workflow"

## Monitoring

### Check Container Status

```bash
cd ~/hbdrop
docker-compose ps
```

### View Logs

```bash
# All containers
docker-compose logs -f

# Specific container
docker-compose logs -f webapp
docker-compose logs -f baileys
docker-compose logs -f postgres
```

### Check Application Health

```bash
curl http://localhost:5007/health
```

Or visit in browser:
- Local: http://localhost:5007
- Public: https://hbdrop.step0fail.com

## Troubleshooting

### Deployment Fails

1. **Check GitHub Actions logs**:
   - Go to Actions tab in your repository
   - Click on the failed workflow
   - Review the error messages

2. **SSH Connection Issues**:
   ```bash
   # Test SSH connection from your local machine
   ssh -i ~/.ssh/hbdrop_deploy your_user@your_server
   ```

3. **Docker Build Fails**:
   ```bash
   # SSH to server and check logs
   cd ~/hbdrop
   docker-compose logs
   ```

### Container Won't Start

```bash
# Check container logs
docker-compose logs webapp

# Rebuild without cache
docker-compose down
docker-compose build --no-cache
docker-compose up -d
```

### Database Issues

```bash
# Access PostgreSQL
docker-compose exec postgres psql -U hbdrop_user -d hbdrop

# Reset database (WARNING: destroys all data)
docker-compose down -v
docker-compose up -d
```

### WhatsApp Connection Lost

```bash
# Restart Baileys service
docker-compose restart baileys

# Check logs
docker-compose logs -f baileys
```

## Rollback

If a deployment causes issues, you can rollback:

```bash
# On the server
cd ~/hbdrop
git checkout <previous-commit-hash>
docker-compose down
docker-compose build --no-cache
docker-compose up -d
```

## Backup

### Backup WhatsApp Auth Data

```bash
cd ~/hbdrop
docker-compose exec baileys tar czf /tmp/auth_backup.tar.gz /app/auth_info
docker cp hbdrop-baileys:/tmp/auth_backup.tar.gz ./auth_backup_$(date +%Y%m%d).tar.gz
```

### Backup Database

```bash
cd ~/hbdrop
docker-compose exec postgres pg_dump -U hbdrop_user hbdrop > backup_$(date +%Y%m%d).sql
```

## Security Notes

1. **Never commit secrets**: The `.env` file is in `.gitignore`
2. **Rotate keys regularly**: Update SSH keys and API keys periodically
3. **Use strong passwords**: For PostgreSQL and other services
4. **Keep updated**: Regularly update Docker images and dependencies
5. **Monitor logs**: Check for suspicious activity

## Support

If you encounter issues:

1. Check the troubleshooting section above
2. Review GitHub Actions workflow logs
3. Check Docker container logs on the server
4. Verify all secrets are correctly configured

## Files Created

- `.github/workflows/deploy.yml` - GitHub Actions workflow
- `deploy.sh` - Deployment script (optional, for manual use)
- `.env.example` - Environment variables template
- `DEPLOYMENT.md` - This documentation file
