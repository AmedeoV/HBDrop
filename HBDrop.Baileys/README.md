# HBDrop Baileys WhatsApp Service

Node.js microservice using Baileys to connect to WhatsApp Web.

## Setup

1. **Install Node.js** (if not already installed):
   - Download from: https://nodejs.org/
   - Version 18 or higher recommended

2. **Install dependencies**:
   ```bash
   cd HBDrop.Baileys
   npm install
   ```

3. **Start the service**:
   ```bash
   npm start
   ```

## First Time Setup

When you start the service for the first time:
1. A QR code will appear in the terminal
2. Open WhatsApp on your phone
3. Go to: **Settings → Linked Devices → Link a Device**
4. Scan the QR code
5. Connection will be saved in `./auth_info` folder

## API Endpoints

### Health Check
```bash
GET http://localhost:3000/health
```

Response:
```json
{
  "status": "ok",
  "connected": true,
  "needsQR": false
}
```

### Get QR Code
```bash
GET http://localhost:3000/qr
```

### Send Message
```bash
POST http://localhost:3000/send
Content-Type: application/json

{
  "phone": "+1234567890",
  "message": "Hello from HBDrop!"
}
```

### Logout
```bash
POST http://localhost:3000/logout
```

## Authentication

- Session is saved in `./auth_info/` folder
- QR code only needed on first run
- Reconnects automatically if connection drops
- To reset: delete `./auth_info/` folder

## Advantages over Selenium

✅ Much lighter (no browser)
✅ Faster message sending
✅ Better for multi-user scenarios
✅ Persistent connection via websocket
✅ No ChromeDriver needed
