# HBDrop Deployment - Quick Reference

## 🚀 Quick Setup (First Time)

### 1. On WSL2 Server
```bash
# Copy and run server setup script
wget https://raw.githubusercontent.com/YOUR_USERNAME/HBDrop/main/server-setup.sh
chmod +x server-setup.sh
./server-setup.sh
```

### 2. On GitHub
Add these secrets (Settings → Secrets and variables → Actions):

```bash
# Generate SSH key
ssh-keygen -t ed25519 -C "github-hbdrop" -f ~/.ssh/hbdrop_deploy
ssh-copy-id -i ~/.ssh/hbdrop_deploy.pub user@server

# Generate encryption key
openssl rand -base64 32
```

| Secret | Value |
|--------|-------|
| `SSH_PRIVATE_KEY` | Contents of `~/.ssh/hbdrop_deploy` |
| `SSH_USER` | Your WSL2 username |
| `SSH_HOST` | Your server IP |
| `DEPLOY_PATH` | `/home/username/hbdrop` |
| `ENCRYPTION_MASTER_KEY` | From openssl command |
| `GIPHY_API_KEY` | From https://developers.giphy.com/ |
| `POSTGRES_PASSWORD` | Secure PostgreSQL password |

### 3. Deploy
```bash
git add .
git commit -m "Initial deployment setup"
git push origin main
```

## 📋 Common Commands

### On Server (WSL2)

```bash
# Navigate to app directory
cd ~/hbdrop

# View container status
docker-compose ps

# View logs
docker-compose logs -f webapp      # Web app
docker-compose logs -f baileys     # WhatsApp service
docker-compose logs -f postgres    # Database

# Restart specific service
docker-compose restart webapp
docker-compose restart baileys

# Restart all services
docker-compose restart

# Stop all services
docker-compose down

# Start all services
docker-compose up -d

# Rebuild and restart
docker-compose down
docker-compose build --no-cache
docker-compose up -d

# Clean up
docker system prune -a
```

### On Local Machine

```bash
# Deploy to production
git push origin main

# Manual trigger (GitHub Actions UI)
# Go to: Actions → Deploy to Production → Run workflow

# Test SSH connection
ssh -i ~/.ssh/hbdrop_deploy user@server

# Manual deployment
scp -r . user@server:~/hbdrop/
ssh user@server "cd ~/hbdrop && docker-compose down && docker-compose build && docker-compose up -d"
```

## 🔍 Troubleshooting

### Container won't start
```bash
docker-compose logs webapp
docker-compose down && docker-compose up -d
```

### Database connection issues
```bash
docker-compose exec postgres psql -U hbdrop_user -d hbdrop
# Check connection string in docker-compose.yml
```

### WhatsApp disconnected
```bash
docker-compose restart baileys
docker-compose logs -f baileys
# Re-scan QR code if needed
```

### Deployment fails
```bash
# Check GitHub Actions logs
# Verify SSH connection
ssh user@server

# Check server space
df -h

# Check Docker status
docker ps -a
```

## 🌐 URLs

- **Production**: https://hbdrop.step0fail.com
- **Direct (Server)**: http://server-ip:5007
- **Health Check**: http://server-ip:5007/health

## 📊 Monitoring

```bash
# Container resources
docker stats

# Disk usage
docker system df

# Container health
docker-compose ps
curl http://localhost:5007/health
```

## 🔐 Security

```bash
# Update SSL certificate (every 90 days)
sudo certbot renew

# Rotate secrets (GitHub Secrets page)
# Generate new encryption key
openssl rand -base64 32

# Check firewall
sudo ufw status
```

## 💾 Backup & Restore

### Backup
```bash
# Database
docker-compose exec postgres pg_dump -U hbdrop_user hbdrop > backup.sql

# WhatsApp auth
docker cp hbdrop-baileys:/app/auth_info ./auth_backup
```

### Restore
```bash
# Database
docker-compose exec -T postgres psql -U hbdrop_user -d hbdrop < backup.sql

# WhatsApp auth
docker cp ./auth_backup/. hbdrop-baileys:/app/auth_info
docker-compose restart baileys
```

## 🆘 Emergency Rollback

```bash
cd ~/hbdrop
git log --oneline -10              # Find previous commit
git checkout <commit-hash>
docker-compose down
docker-compose build --no-cache
docker-compose up -d
```

## 📞 Health Checks

```bash
# Application
curl http://localhost:5007/health

# Baileys service
curl http://localhost:3000/health

# Database
docker-compose exec postgres pg_isready -U hbdrop_user

# All services
docker-compose ps
```

## 🔄 Update Dependencies

```bash
# .NET packages (on next deployment)
# Update HBDrop.WebApp/HBDrop.WebApp.csproj

# Node packages for Baileys
cd HBDrop.Baileys
npm update
git add package.json package-lock.json
git commit -m "Update Baileys dependencies"
git push
```

## 📝 Logs Location

- **GitHub Actions**: Repository → Actions → Workflow run
- **Server**: `~/hbdrop/deploy.log` (if using deploy.sh)
- **Docker**: `docker-compose logs`
- **Nginx**: `/var/log/nginx/access.log` and `/var/log/nginx/error.log`

---

For detailed information, see [DEPLOYMENT.md](./DEPLOYMENT.md)
