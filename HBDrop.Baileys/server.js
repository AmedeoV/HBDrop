const express = require('express');
const { default: makeWASocket, useMultiFileAuthState, DisconnectReason, fetchLatestBaileysVersion } = require('@whiskeysockets/baileys');
const QRCode = require('qrcode');
const P = require('pino');
const fs = require('fs');
const path = require('path');

const app = express();
app.use(express.json());

// Store multiple user sessions: Map<userId, sessionData>
const userSessions = new Map();

const AUTH_FOLDER_BASE = './auth_info';
const MAX_RETRIES = 3;

const logger = P({ level: 'silent' });

// Ensure base auth folder exists
if (!fs.existsSync(AUTH_FOLDER_BASE)) {
    fs.mkdirSync(AUTH_FOLDER_BASE, { recursive: true });
}

// Get user's auth folder
function getUserAuthFolder(userId) {
    const folder = path.join(AUTH_FOLDER_BASE, userId.toString());
    if (!fs.existsSync(folder)) {
        fs.mkdirSync(folder, { recursive: true });
    }
    return folder;
}

// Initialize WhatsApp connection for a specific user
async function connectToWhatsApp(userId, usePairingCode = false, phoneNumber = null) {
    try {
        console.log(`[${userId}]  Starting WhatsApp connection...`);
        if (usePairingCode && phoneNumber) {
            console.log(`[${userId}]  Using pairing code mode for ${phoneNumber}`);
        }
        
        const authFolder = getUserAuthFolder(userId);
        const { state, saveCreds } = await useMultiFileAuthState(authFolder);
        const { version } = await fetchLatestBaileysVersion();
        
        console.log(`[${userId}]  Using Baileys version: ${version.join('.')}`);
        
        const sock = makeWASocket({
            version,
            auth: state,
            logger,
            browser: ['HBDrop', 'Chrome', '1.0.0'],
            syncFullHistory: false,
            markOnlineOnConnect: false,
            // Enable pairing code if requested
            printQRInTerminal: !usePairingCode
        });

        // Initialize session data
        const sessionData = {
            sock,
            qrCodeData: null,
            pairingCode: null,
            isConnected: false,
            connectionAttempts: 0,
            userId,
            phoneNumber: phoneNumber || null,
            usePairingCode,
            pairingCodeRequested: false
        };
        
        userSessions.set(userId, sessionData);

        sock.ev.on('creds.update', saveCreds);

        sock.ev.on('connection.update', async (update) => {
            const { connection, lastDisconnect, qr, isNewLogin } = update;
            
            // Request pairing code on QR event if using pairing code mode
            if (qr && usePairingCode && phoneNumber && !sessionData.pairingCodeRequested) {
                console.log(`[${userId}]  QR event received, requesting pairing code instead...`);
                sessionData.pairingCodeRequested = true;
                
                const formats = [];
                
                console.log(`[${userId}]  Original number: ${phoneNumber}`);
                
                // Get digits only first
                const digitsOnly = phoneNumber.replace(/[^0-9]/g, '');
                console.log(`[${userId}]  Digits only: ${digitsOnly}`);
                
                // For Irish numbers starting with 353, try ONLY the domestic format first
                // This is likely what was used during WhatsApp registration
                if (digitsOnly.startsWith('353') && digitsOnly.length === 12) {
                    const domestic = digitsOnly.substring(3); // 899548661
                    formats.push(domestic); // 899548661 - try WITHOUT leading 0 first!
                    console.log(`[${userId}]  Detected Irish number, trying domestic format WITHOUT leading 0: ${domestic}`);
                }
                
                // Then try the exact format as provided
                formats.push(phoneNumber);
                
                // Try with digits only
                if (!formats.includes(digitsOnly)) {
                    formats.push(digitsOnly);
                }
                
                console.log(`[${userId}]  Will try ${formats.length} formats:`, formats);
                
                for (let i = 0; i < formats.length; i++) {
                    const format = formats[i];
                    try {
                        console.log(`[${userId}]  Trying format ${i + 1}/${formats.length}: "${format}"`);
                        const code = await sock.requestPairingCode(format);
                        sessionData.pairingCode = code;
                        console.log(`[${userId}] ✅ SUCCESS! Pairing code generated: ${code} using format: ${format}`);
                        return;
                    } catch (err) {
                        console.log(`[${userId}]  Format ${format} failed:`, err.message);
                        if (i === formats.length - 1) {
                            // Last attempt failed
                            console.error(`[${userId}]  All formats failed`);
                            sessionData.pairingCodeError = 'All phone number formats failed. Please verify your number.';
                        }
                    }
                }
            }
            
            if (qr && !usePairingCode) {
                console.log(`[${userId}]  QR Code generated`);
                try {
                    sessionData.qrCodeData = await QRCode.toDataURL(qr);
                } catch (err) {
                    console.error(`[${userId}]  Error generating QR code:`, err);
                }
            }
            
            if (connection === 'close') {
                const statusCode = lastDisconnect?.error?.output?.statusCode;
                const shouldReconnect = statusCode !== DisconnectReason.loggedOut;
                const errorMsg = lastDisconnect?.error?.message || 'Unknown error';
                
                // Log detailed error information
                let statusDescription = 'Unknown';
                switch(statusCode) {
                    case 401: statusDescription = 'Logged out by user or another device'; break;
                    case 403: statusDescription = 'WhatsApp Web access forbidden'; break;
                    case 408: statusDescription = 'Connection timeout'; break;
                    case 411: statusDescription = 'Conflict - another connection'; break;
                    case 428: statusDescription = 'Connection lost'; break;
                    case 440: statusDescription = 'Connection replaced'; break;
                    case 500: statusDescription = 'Internal WhatsApp error'; break;
                    case 515: statusDescription = 'Restart required'; break;
                    default: statusDescription = `Code ${statusCode}`;
                }
                
                console.log(`[${userId}] ⚠️  Connection closed: ${statusDescription}`);
                console.log(`[${userId}]    Status Code: ${statusCode}`);
                console.log(`[${userId}]    Error: ${errorMsg}`);
                console.log(`[${userId}]    Will Reconnect: ${shouldReconnect}`);
                
                sessionData.isConnected = false;
                sessionData.qrCodeData = null;
                
                if (shouldReconnect) {
                    sessionData.connectionAttempts++;
                    
                    if (sessionData.connectionAttempts <= MAX_RETRIES) {
                        console.log(`[${userId}] 🔄 Reconnecting... (Attempt ${sessionData.connectionAttempts}/${MAX_RETRIES})`);
                        setTimeout(() => connectToWhatsApp(userId), 2000);
                    } else {
                        console.log(`[${userId}] ❌ Max reconnection attempts reached`);
                        userSessions.delete(userId);
                    }
                } else {
                    console.log(`[${userId}] 🚪 Logged out - cleaning up session`);
                    userSessions.delete(userId);
                    
                    // Delete auth files after logout
                    setTimeout(() => {
                        try {
                            const files = fs.readdirSync(authFolder);
                            files.forEach(file => {
                                try {
                                    fs.unlinkSync(path.join(authFolder, file));
                                } catch (err) {
                                    console.error(`[${userId}] Error deleting file ${file}:`, err);
                                }
                            });
                            console.log(`[${userId}] 🗑️  Auth files deleted`);
                        } catch (err) {
                            console.error(`[${userId}] Error cleaning auth folder:`, err);
                        }
                    }, 1000);
                }
            } else if (connection === 'open') {
                console.log(`[${userId}] ✅ WhatsApp connected successfully!`);
                sessionData.isConnected = true;
                sessionData.qrCodeData = null;
                sessionData.connectionAttempts = 0;
                
                // Get phone number
                try {
                    const phoneNumber = sock.user?.id?.split(':')[0] || 'Unknown';
                    sessionData.phoneNumber = phoneNumber;
                    console.log(`[${userId}] 📞 Phone: ${phoneNumber}`);
                } catch (err) {
                    console.error(`[${userId}] Error getting phone number:`, err);
                }
            } else if (connection === 'connecting') {
                console.log(`[${userId}]  Connecting...`);
            }
        });

        return sessionData;
    } catch (error) {
        console.error(`[${userId}]  Error in connectToWhatsApp:`, error);
        throw error;
    }
}

// Get or create user session
async function getUserSession(userId) {
    let session = userSessions.get(userId);
    
    if (!session) {
        session = await connectToWhatsApp(userId);
    }
    
    return session;
}

// API Endpoints

// Health check
app.get('/health', (req, res) => {
    res.json({
        status: 'ok',
        activeSessions: userSessions.size,
        users: Array.from(userSessions.keys())
    });
});

// Get QR code for user
app.get('/qr/:userId', async (req, res) => {
    try {
        const userId = req.params.userId;
        console.log(`[${userId}] 📥 QR code request received`);
        
        const session = await getUserSession(userId);
        
        if (session.isConnected) {
            console.log(`[${userId}] ⚠️  Already connected, cannot generate QR`);
            return res.json({ 
                success: false, 
                message: 'WhatsApp is already connected',
                connected: true 
            });
        }
        
        if (!session.qrCodeData) {
            console.log(`[${userId}] ⏳ QR code not yet generated, please wait...`);
            return res.json({ 
                success: false, 
                message: 'QR code not yet generated. Please wait...',
                qrCode: null 
            });
        }
        
        console.log(`[${userId}] ✅ QR code sent successfully`);
        res.json({ 
            success: true, 
            qrCode: session.qrCodeData 
        });
    } catch (error) {
        console.error(`[QR] ❌ Error:`, error);
        res.status(500).json({ 
            success: false, 
            message: error.message 
        });
    }
});

// Request pairing code for user (alternative to QR)
app.post('/pairing-code/:userId', async (req, res) => {
    try {
        const userId = req.params.userId;
        const { phoneNumber } = req.body;
        
        if (!phoneNumber) {
            return res.status(400).json({
                success: false,
                message: 'Phone number is required (e.g., "1234567890")'
            });
        }
        
        console.log(`[${userId}] 📥 Pairing code request for ${phoneNumber}`);
        
        // Check if already connected
        const existingSession = userSessions.get(userId);
        if (existingSession?.isConnected) {
            console.log(`[${userId}] ⚠️  Already connected`);
            return res.json({
                success: false,
                message: 'WhatsApp is already connected',
                connected: true
            });
        }
        
        // Delete old session if exists
        if (existingSession) {
            console.log(`[${userId}]  Cleaning up old session...`);
            userSessions.delete(userId);
        }
        
        // Create new session with pairing code
        const session = await connectToWhatsApp(userId, true, phoneNumber);
        
        // Wait for pairing code to be generated (it happens on QR event)
        let attempts = 0;
        while (!session.pairingCode && attempts < 20) {
            await new Promise(resolve => setTimeout(resolve, 500));
            attempts++;
        }
        
        if (!session.pairingCode) {
            console.log(`[${userId}] ⏳ Pairing code not yet generated after ${attempts * 500}ms`);
            return res.json({
                success: false,
                message: 'Pairing code not yet generated. Please try again in a moment.',
                pairingCode: null
            });
        }
        
        console.log(`[${userId}] ✅ Pairing code sent successfully`);
        res.json({
            success: true,
            pairingCode: session.pairingCode,
            message: 'Enter this code in WhatsApp > Linked Devices > Link a Device > Link with phone number instead'
        });
    } catch (error) {
        console.error(`[Pairing] ❌ Error:`, error);
        res.status(500).json({
            success: false,
            message: error.message
        });
    }
});

// Check connection status for user
app.get('/status/:userId', async (req, res) => {
    try {
        const userId = req.params.userId;
        const session = userSessions.get(userId);
        
        if (!session) {
            console.log(`[${userId}] ℹ️  Status check: No session found`);
            return res.json({ 
                isConnected: false,
                phoneNumber: null,
                message: 'No session found. Please connect WhatsApp.' 
            });
        }
        
        const statusMessage = session.isConnected ? 
            `Connected as ${session.phoneNumber || 'Unknown'}` : 
            'Not connected';
        
        console.log(`[${userId}] 📊 Status check: ${statusMessage}`);
        
        res.json({ 
            isConnected: session.isConnected,
            phoneNumber: session.phoneNumber,
            hasQrCode: !!session.qrCodeData,
            hasPairingCode: !!session.pairingCode,
            pairingCode: session.pairingCode,
            usePairingCode: session.usePairingCode || false,
            message: statusMessage 
        });
    } catch (error) {
        console.error(`[Status] ❌ Error:`, error);
        res.status(500).json({ 
            isConnected: false, 
            message: error.message 
        });
    }
});

// Send message for user
app.post('/send/:userId', async (req, res) => {
    try {
        const userId = req.params.userId;
        const { phone, message, gifUrl } = req.body;
        
        if (!phone || !message) {
            return res.status(400).json({ 
                success: false, 
                message: 'Phone number and message are required' 
            });
        }
        
        const session = userSessions.get(userId);
        
        if (!session || !session.isConnected) {
            return res.status(400).json({ 
                success: false, 
                message: 'WhatsApp not connected for this user' 
            });
        }
        
        // Check if it's a group (ends with @g.us) or individual (ends with @s.whatsapp.net)
        let formattedPhone;
        if (phone.includes('@g.us')) {
            formattedPhone = phone;
        } else if (phone.includes('@s.whatsapp.net')) {
            formattedPhone = phone;
        } else {
            formattedPhone = phone.replace(/[^0-9]/g, '') + '@s.whatsapp.net';
        }
        
        console.log(`[${userId}]  Sending message to ${phone}...`);
        
        // If GIF URL is provided, send it first, then the text message
        if (gifUrl) {
            try {
                console.log(`[${userId}]  Sending GIF from URL: ${gifUrl}`);
                await session.sock.sendMessage(formattedPhone, {
                    video: { url: gifUrl },
                    gifPlayback: true,
                    caption: message
                });
                console.log(`[${userId}]  GIF with caption sent successfully`);
            } catch (gifError) {
                console.error(`[${userId}]  Error sending GIF, falling back to text:`, gifError.message);
                // Fallback to text-only message if GIF fails
                await session.sock.sendMessage(formattedPhone, { text: message });
            }
        } else {
            // Send text-only message
            await session.sock.sendMessage(formattedPhone, { text: message });
        }
        
        console.log(`[${userId}]  Message sent successfully`);
        
        res.json({ 
            success: true, 
            message: 'Message sent successfully' 
        });
    } catch (error) {
        console.error('Error sending message:', error);
        res.status(500).json({ 
            success: false, 
            message: error.message 
        });
    }
});

// Get groups for user
app.get('/groups/:userId', async (req, res) => {
    try {
        const userId = req.params.userId;
        const session = userSessions.get(userId);
        
        if (!session || !session.isConnected) {
            return res.status(400).json({ 
                success: false, 
                message: 'WhatsApp not connected' 
            });
        }
        
        console.log(`[${userId}]  Fetching WhatsApp groups...`);
        const groups = await session.sock.groupFetchAllParticipating();
        
        const groupList = Object.values(groups).map(group => ({
            id: group.id,
            name: group.subject,
            participants: group.participants?.length || 0,
            owner: group.owner,
            description: group.desc || '',
            createdAt: group.creation
        }));
        
        groupList.sort((a, b) => b.createdAt - a.createdAt);
        
        console.log(`[${userId}]  Found ${groupList.length} groups`);
        
        res.json({ 
            success: true, 
            groups: groupList 
        });
    } catch (error) {
        console.error('Error fetching groups:', error);
        res.status(500).json({ 
            success: false, 
            message: error.message 
        });
    }
});

// Logout user
app.post('/logout/:userId', async (req, res) => {
    try {
        const userId = req.params.userId;
        const session = userSessions.get(userId);
        
        if (session && session.sock) {
            console.log(`[${userId}]  Logging out...`);
            await session.sock.logout();
            await session.sock.end();
        }
        
        userSessions.delete(userId);
        
        // Delete auth files
        const authFolder = getUserAuthFolder(userId);
        setTimeout(() => {
            if (fs.existsSync(authFolder)) {
                try {
                    const files = fs.readdirSync(authFolder);
                    files.forEach(file => {
                        try {
                            fs.unlinkSync(path.join(authFolder, file));
                        } catch (err) {
                            console.error(`[${userId}] Error deleting file:`, err);
                        }
                    });
                    fs.rmdirSync(authFolder);
                    console.log(`[${userId}]   Auth folder deleted`);
                } catch (err) {
                    console.error(`[${userId}] Error cleaning auth folder:`, err);
                }
            }
        }, 1000);
        
        res.json({ 
            success: true, 
            message: 'Logged out successfully' 
        });
    } catch (error) {
        console.error('Error logging out:', error);
        res.status(500).json({ 
            success: false, 
            message: error.message 
        });
    }
});

const PORT = process.env.PORT || 3000;
app.listen(PORT, () => {
    console.log(`\n ================================================`);
    console.log(`   Baileys WhatsApp Multi-User Service`);
    console.log(`   Running on port ${PORT}`);
    console.log(`   Supporting multiple concurrent user sessions`);
    console.log(`   Health check: http://localhost:${PORT}/health`);
    console.log(`================================================\n`);
});
