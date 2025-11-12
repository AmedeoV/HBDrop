const { default: makeWASocket, useMultiFileAuthState, DisconnectReason, fetchLatestBaileysVersion } = require('@whiskeysockets/baileys');
const QRCode = require('qrcode-terminal');
const P = require('pino');

async function testConnection() {
    console.log('🔍 Testing Baileys connection...\n');
    
    try {
        // Get latest version info
        const { version, isLatest } = await fetchLatestBaileysVersion();
        console.log(`📦 Using Baileys version: ${version.join('.')}`);
        console.log(`✅ Is latest: ${isLatest}\n`);
        
        const { state, saveCreds } = await useMultiFileAuthState('./auth_info_test');
        
        const sock = makeWASocket({
            auth: state,
            logger: P({ level: 'info' }),  // Changed to info to see more details
            browser: ['Chrome (Windows)', '', ''],
            syncFullHistory: false
        });

        sock.ev.on('creds.update', saveCreds);

        sock.ev.on('connection.update', (update) => {
            const { connection, lastDisconnect, qr } = update;
            
            console.log('📡 Connection update:', { connection, hasQR: !!qr });
            
            if (qr) {
                console.log('\n📱 ==================== QR CODE ====================\n');
                QRCode.generate(qr, { small: true });
                console.log('\n===================================================');
                console.log('   Scan this QR code with WhatsApp on your phone!');
                console.log('===================================================\n');
            }

            if (connection === 'close') {
                const statusCode = lastDisconnect?.error?.output?.statusCode;
                const reason = lastDisconnect?.error;
                
                console.log('\n❌ Connection closed');
                console.log('   Status code:', statusCode);
                console.log('   Error:', reason);
                console.log('   Should reconnect:', statusCode !== DisconnectReason.loggedOut);
                
                if (statusCode === 405) {
                    console.log('\n⚠️  Status 405 usually means:');
                    console.log('   1. Network/firewall blocking connection');
                    console.log('   2. WhatsApp servers temporarily unavailable');
                    console.log('   3. Try checking your internet connection');
                    console.log('   4. Try again in a few minutes\n');
                }
                
                process.exit(1);
            } else if (connection === 'open') {
                console.log('\n✅ ==================== CONNECTED! ====================');
                console.log('   WhatsApp is now connected!');
                console.log('   You can close this and run the main server.');
                console.log('========================================================\n');
                
                setTimeout(() => {
                    console.log('Closing test connection...');
                    sock.end();
                    process.exit(0);
                }, 3000);
            } else if (connection === 'connecting') {
                console.log('🔄 Connecting to WhatsApp...');
            }
        });

    } catch (error) {
        console.error('❌ Error:', error.message);
        console.error('Stack:', error.stack);
        process.exit(1);
    }
}

console.log('=== Baileys Connection Test ===\n');
testConnection();
