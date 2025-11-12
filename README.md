# HBDrop 🎉# HBDrop - Happy Birthday Drop 🎉



A birthday reminder and WhatsApp messaging application built with ASP.NET Core Blazor and integrated WhatsApp messaging via Baileys.Automated birthday wishes via WhatsApp with future support for Telegram and other platforms.



## Features## 🎯 Project Overview



- 📅 Birthday management and trackingHBDrop is a web application that helps you never forget a birthday again! It automatically sends personalized WhatsApp messages to your contacts on their birthdays.

- 📱 Automated WhatsApp birthday messages

- 🎨 GIF support via Giphy integration### Features

- 👥 Group messaging support- ✅ WhatsApp integration via Baileys (no official API required)

- 🔐 Secure data encryption- ✅ Multi-user support with encrypted session storage

- 🌐 Web-based interface- ✅ PostgreSQL database for contacts and birthdays

- 🐳 Docker containerized deployment- ✅ Automated daily birthday checks

- ✅ Manual message sending

## Tech Stack- ✅ Message delivery tracking

- ✅ Docker deployment ready

- **Frontend**: Blazor Server (ASP.NET Core 9.0)- ✅ Beautiful birthday-themed landing page

- **Backend**: ASP.NET Core Web API- ✅ Interactive animated demo

- **Database**: PostgreSQL 16- 🔄 User dashboard (in progress)

- **WhatsApp Integration**: Baileys (Node.js)- 🔄 Contact management UI (in progress)

- **Containerization**: Docker & Docker Compose- 🔄 Telegram support (planned)

- **CI/CD**: GitHub Actions- 🔄 SMS support (planned)



## Project Structure## 🏗️ Architecture



```### Components

HBDrop/1. **HBDrop.WebApp** - ASP.NET Core 9.0 Blazor Server application

├── HBDrop.WebApp/          # Main Blazor web application2. **HBDrop.Baileys** - Node.js service for WhatsApp Web API

├── HBDrop.Baileys/         # Node.js WhatsApp service3. **PostgreSQL** - Database for users, contacts, birthdays, messages

├── HBDrop/                 # Legacy console app4. **Hangfire** - Background job processing

├── .github/workflows/      # CI/CD workflows

└── docker-compose.yml      # Production container orchestration### Technology Stack

```- **Backend**: ASP.NET Core 9.0 (Blazor Server)

- **Database**: PostgreSQL 16

## Quick Start- **Authentication**: ASP.NET Core Identity

- **Background Jobs**: Hangfire

### Prerequisites- **WhatsApp**: Baileys (@whiskeysockets/baileys)

- **Containerization**: Docker & Docker Compose

- Docker & Docker Compose

- .NET 9.0 SDK (for local development)## 🚀 Quick Start

- Node.js 20+ (for local development)

- PostgreSQL 16 (or use Docker)### Prerequisites

- .NET 9.0 SDK

### Local Development- Node.js 18+

- Docker Desktop

1. **Clone the repository**- Git

   ```bash

   git clone https://github.com/YOUR_USERNAME/HBDrop.git### Development Setup

   cd HBDrop

   ```1. **Clone the repository**

   ```bash

2. **Set up environment variables**   git clone <your-repo-url>

   ```bash   cd HBDrop

   cp .env.example .env   ```

   # Edit .env with your values

   ```2. **Start PostgreSQL**

   ```powershell

3. **Start with Docker Compose**   docker-compose -f docker-compose.dev.yml up -d

   ```bash   ```

   docker-compose -f docker-compose.dev.yml up -d

   ```3. **Start Baileys WhatsApp Service**

   ```powershell

4. **Access the application**   cd HBDrop.Baileys

   - Web App: http://localhost:5007   npm install

   - Baileys API: http://localhost:3000   npm start

   ```

### Production Deployment

4. **Run Web Application**

See [DEPLOYMENT.md](./DEPLOYMENT.md) for comprehensive deployment instructions.   ```powershell

   cd HBDrop.WebApp

**Quick Deploy:**   dotnet restore

```bash   dotnet run

# On WSL2 server, run the setup script   ```

./server-setup.sh

5. **Access the application**

# Configure GitHub Secrets (see DEPLOYMENT.md)   - Web App: https://localhost:5001

   - Hangfire Dashboard: https://localhost:5001/hangfire

# Push to deploy   - Baileys Health: http://localhost:3000/health

git push origin main

```## 📚 Documentation



## Configuration- **[QUICKSTART_WEB.md](QUICKSTART_WEB.md)** - Quick reference guide

- **[WEB_APP_PROGRESS.md](WEB_APP_PROGRESS.md)** - Detailed progress and architecture

### Environment Variables- **[BAILEYS_QUICKSTART.md](BAILEYS_QUICKSTART.md)** - WhatsApp service setup



| Variable | Description | Required |## 🗄️ Database Schema

|----------|-------------|----------|

| `ENCRYPTION_MASTER_KEY` | Master key for data encryption | Yes |### Main Tables

| `GIPHY_API_KEY` | API key for GIF search | Yes |- **AspNetUsers** - User accounts with Identity

| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | Yes |- **Contacts** - Contact information and phone numbers

| `Baileys__ApiUrl` | Baileys service URL | Yes |- **Birthdays** - Birthday dates and custom messages

- **Messages** - Message history and delivery status

See `.env.example` for a complete template.- **WhatsAppSessions** - Encrypted WhatsApp authentication data



### Database Setup## 🔐 Security



The application automatically runs migrations on startup. For manual migration:- Passwords hashed with ASP.NET Core Identity (PBKDF2)

- WhatsApp sessions encrypted with AES-256

```bash- User-specific encryption keys derived from user ID + master key

# Inside the webapp container- HTTPS enforced in production

dotnet ef database update- Role-based access control

```

## 🐳 Docker Deployment

## Development

### Production Deployment

### Running Locally (Without Docker)```bash

# Generate encryption key first

1. **Start PostgreSQL**# Set ENCRYPTION_MASTER_KEY environment variable

   ```bash

   docker run -d -p 5432:5432 \docker-compose up -d

     -e POSTGRES_DB=hbdrop \```

     -e POSTGRES_USER=hbdrop_user \

     -e POSTGRES_PASSWORD=password \This will start:

     postgres:16-alpine- PostgreSQL database

   ```- Baileys WhatsApp service

- ASP.NET Core web application

2. **Start Baileys service**

   ```bash## 📋 Current Status

   cd HBDrop.Baileys

   npm install### ✅ Completed

   node server.js- Project structure and configuration

   ```- Database models and migrations

- PostgreSQL integration

3. **Start Web App**- ASP.NET Core Identity setup

   ```bash- WhatsApp service integration

   cd HBDrop.WebApp- Session encryption service

   dotnet run- Hangfire background jobs setup

   ```- Docker development environment



### Adding Migrations### 🔄 In Progress

- Blazor UI components

```bash- Authentication pages

cd HBDrop.WebApp- Contact management pages

dotnet ef migrations add YourMigrationName- WhatsApp connection page

```- Message history



## API Documentation### ⏳ Planned

- Daily birthday check background job

### Baileys API Endpoints- Email notifications

- Telegram integration

- `POST /send-message` - Send WhatsApp message- SMS integration

- `GET /health` - Health check- Multi-language support

- `GET /qr` - Get QR code for authentication

## 🤝 Contributing

### Web App Endpoints

This is a personal project, but suggestions and feedback are welcome!

- `/` - Home page

- `/birthdays` - Birthday management## 📄 License

- `/identity/*` - Authentication pages

[Your License Here]

## Testing

## 👤 Author

```bash

# Run tests (when implemented)[Your Name]

dotnet test

```---



## Monitoring## 🛠️ Development Commands



### View Logs### Database Migrations

```powershell

```bash# Create migration

# All servicesdotnet ef migrations add MigrationName

docker-compose logs -f

# Apply migrations

# Specific servicedotnet ef database update

docker-compose logs -f webapp

docker-compose logs -f baileys# Rollback migration

docker-compose logs -f postgresdotnet ef database update PreviousMigrationName

```

# Remove last migration (if not applied)

### Health Checksdotnet ef migrations remove

```

```bash

# Web app### Docker Commands

curl http://localhost:5007/health```powershell

# Start dev environment

# Baileysdocker-compose -f docker-compose.dev.yml up -d

curl http://localhost:3000/health

```# Stop dev environment

docker-compose -f docker-compose.dev.yml down

## Troubleshooting

# View logs

See [DEPLOYMENT_QUICKREF.md](./DEPLOYMENT_QUICKREF.md) for common issues and solutions.docker-compose -f docker-compose.dev.yml logs -f

```

### Common Issues

### Testing Baileys

**WhatsApp not connecting**```powershell

- Check Baileys logs: `docker-compose logs -f baileys`# Check health

- Restart service: `docker-compose restart baileys`curl http://localhost:3000/health

- Scan QR code again

# Get QR code

**Database connection errors**curl http://localhost:3000/qr

- Verify PostgreSQL is running: `docker-compose ps postgres`

- Check connection string in `appsettings.json`# Send test message

curl -X POST http://localhost:3000/send -H "Content-Type: application/json" -d '{"phone":"+1234567890","message":"Test"}'

**Build failures**```

- Clear Docker cache: `docker-compose build --no-cache`

- Check Docker disk space: `docker system df`## 📞 Support



## ContributingFor issues or questions:

1. Check the documentation files

1. Fork the repository2. Review the troubleshooting section in QUICKSTART_WEB.md

2. Create a feature branch (`git checkout -b feature/amazing-feature`)3. Check logs: `docker-compose logs` or application console

3. Commit your changes (`git commit -m 'Add amazing feature'`)

4. Push to the branch (`git push origin feature/amazing-feature`)---

5. Open a Pull Request

**Happy Birthday wishes automated! 🎂🎉**

## Security

- Never commit `.env` files or secrets
- Rotate API keys regularly
- Keep dependencies updated
- Use strong passwords for database

## License

[Add your license here]

## Support

For issues and questions:
- Open an issue on GitHub
- Check [DEPLOYMENT.md](./DEPLOYMENT.md) for deployment help
- See [DEPLOYMENT_QUICKREF.md](./DEPLOYMENT_QUICKREF.md) for quick commands

## Acknowledgments

- [Baileys](https://github.com/WhiskeySockets/Baileys) - WhatsApp Web API
- [Giphy](https://giphy.com) - GIF integration
- ASP.NET Core Team
- Docker Community

---

**Live Site**: https://hbdrop.step0fail.com

Made with ❤️ for remembering birthdays
