# Next Steps for HBDrop Deployment

## ✅ Completed
- [x] GitHub Actions automated deployment workflow
- [x] Self-hosted runner on WSL2 configured and running
- [x] Docker containers deployed and running successfully
- [x] Application accessible at http://172.20.199.85:5007
- [x] Nginx reverse proxy configured
- [x] Certbot installed

## 🚀 Remaining Steps

### 1. DNS Configuration

You need to configure DNS for `hbdrop.step0fail.com` to point to your server.

**Option A: Public Server (Recommended for Production)**
If your server has a public IP address accessible from the internet:

```bash
# Get your public IP
wsl bash -c "curl -s ifconfig.me"
```

Then add an A record in your DNS provider:
- **Type:** A
- **Name:** hbdrop
- **Value:** [Your public IP]
- **TTL:** 3600 (or auto)

**Option B: Local Development (Current Setup)**
Currently configured for local testing with hosts file:
- WSL2: Added `172.20.199.85 hbdrop.step0fail.com` to `/etc/hosts`
- Windows: Add to `C:\Windows\System32\drivers\etc\hosts`:
  ```
  172.20.199.85 hbdrop.step0fail.com
  ```

### 2. SSL Certificate Setup

**Important:** SSL certificates require:
- Domain DNS pointing to your public IP
- Ports 80 and 443 accessible from the internet
- Valid domain ownership verification

#### For Public Server:

```bash
# Make sure ports 80/443 are open in your firewall
wsl bash -c "sudo ufw allow 80/tcp && sudo ufw allow 443/tcp"

# Request SSL certificate
wsl bash -c "sudo certbot --nginx -d hbdrop.step0fail.com"
```

Certbot will:
- Automatically verify domain ownership
- Obtain SSL certificate from Let's Encrypt
- Configure Nginx to use HTTPS
- Set up auto-renewal

#### For Local Development:
Use self-signed certificate:

```bash
wsl bash -c "sudo openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout /etc/ssl/private/hbdrop.key \
  -out /etc/ssl/certs/hbdrop.crt \
  -subj '/CN=hbdrop.step0fail.com'"
```

Then update Nginx config to use the certificate.

### 3. Test Automated Deployment

Verify the complete CI/CD pipeline:

```powershell
# Make a small change
cd D:\Projects\HBDrop
echo "# Test deployment" >> README.md

# Commit and push
git add README.md
git commit -m "Test automated deployment"
git push origin main

# Monitor deployment
wsl bash -c "cd ~/hbdrop && watch -n 2 'docker-compose ps'"
```

Check GitHub Actions: https://github.com/AmedeoV/HBDrop/actions

### 4. Production Checklist

Before going live, ensure:

- [ ] All GitHub Secrets are properly configured
- [ ] Database backups are configured
- [ ] Monitoring/logging is set up
- [ ] Firewall rules are configured
- [ ] SSL certificate auto-renewal is working
- [ ] Runner service starts on boot: 
  ```bash
  wsl bash -c "sudo systemctl enable actions.runner.AmedeoV-HBDrop.hbdrop-wsl2-runner"
  ```

### 5. Maintenance Commands

**View logs:**
```bash
wsl bash -c "cd ~/hbdrop && docker-compose logs -f webapp"
wsl bash -c "sudo tail -f /var/log/nginx/hbdrop_access.log"
```

**Restart services:**
```bash
wsl bash -c "cd ~/hbdrop && docker-compose restart webapp"
wsl bash -c "sudo systemctl restart nginx"
```

**Check runner status:**
```bash
wsl bash -c "sudo systemctl status actions.runner.AmedeoV-HBDrop.hbdrop-wsl2-runner"
```

**Database backup:**
```bash
wsl bash -c "cd ~/hbdrop && docker-compose exec postgres pg_dump -U hbdrop_user hbdrop > backup_\$(date +%Y%m%d_%H%M%S).sql"
```

## 📝 Current Access Points

- **Direct Access:** http://172.20.199.85:5007
- **Via Nginx (local):** http://hbdrop.step0fail.com (requires hosts file entry)
- **Hangfire Dashboard:** http://172.20.199.85:5007/hangfire
- **Health Check:** http://172.20.199.85:5007/health

## 🔗 Useful Links

- [GitHub Repository](https://github.com/AmedeoV/HBDrop)
- [GitHub Actions](https://github.com/AmedeoV/HBDrop/actions)
- [Deployment Documentation](./DEPLOYMENT.md)
- [Quick Reference](./DEPLOYMENT_QUICKREF.md)

## 🆘 Troubleshooting

**Deployment fails:**
```bash
# Check runner logs
wsl bash -c "cd ~/actions-runner-hbdrop && tail -50 _diag/Runner_*.log"

# Check container logs
wsl bash -c "cd ~/hbdrop && docker-compose logs --tail=100"
```

**App not accessible:**
```bash
# Check if containers are running
wsl bash -c "cd ~/hbdrop && docker-compose ps"

# Check Nginx status
wsl bash -c "sudo systemctl status nginx"

# Test connection
curl -I http://172.20.199.85:5007/health
```

**Database issues:**
```bash
# Connect to database
wsl bash -c "cd ~/hbdrop && docker-compose exec postgres psql -U hbdrop_user -d hbdrop"

# Reset database (WARNING: Deletes all data)
wsl bash -c "cd ~/hbdrop && docker-compose down -v && docker-compose up -d"
```
