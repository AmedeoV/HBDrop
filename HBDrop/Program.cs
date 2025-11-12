using HBDrop.Services;

namespace HBDrop;

class Program
{
    static async Task Main(string[] args)
    {
        // Handle command-line arguments
        if (args.Length > 0)
        {
            switch (args[0].ToLower())
            {
                case "--check":
                case "-c":
                    await CheckBaileysService();
                    return;
                case "--help":
                case "-h":
                case "?":
                    ShowHelp();
                    return;
            }
        }

        Console.WriteLine("=== WhatsApp Message Sender (Baileys) ===\n");

        // Get message details from user
        Console.Write("Enter the phone number (e.g., +391234567890): ");
        string? phoneNumber = Console.ReadLine();

        Console.Write("Enter the message to send: ");
        string? message = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(message))
        {
            Console.WriteLine("❌ Phone number and message are required!");
            return;
        }

        // Use the Baileys service
        using var whatsapp = new BaileysWhatsAppService();

        try
        {
            // Check if Baileys service is running
            Console.WriteLine("\n🔍 Checking Baileys service...");
            var health = await whatsapp.GetHealthAsync();

            if (health == null)
            {
                Console.WriteLine("❌ Baileys service is not running!");
                Console.WriteLine("\nTo start the service:");
                Console.WriteLine("1. Open a new terminal");
                Console.WriteLine("2. cd d:\\Projects\\HBDrop\\HBDrop.Baileys");
                Console.WriteLine("3. npm install (first time only)");
                Console.WriteLine("4. npm start");
                return;
            }

            Console.WriteLine($"✅ Service is running (Connected: {health.Connected})");

            // Check if we need to scan QR code
            if (!health.Connected && health.NeedsQR)
            {
                Console.WriteLine("\n📱 QR Code is ready!");
                Console.WriteLine("Please check the Baileys service terminal and scan the QR code with your phone.");
                Console.WriteLine("Waiting for you to scan...");
                
                // Wait for connection
                for (int i = 0; i < 30; i++)
                {
                    await Task.Delay(2000);
                    var newHealth = await whatsapp.GetHealthAsync();
                    if (newHealth?.Connected ?? false)
                    {
                        Console.WriteLine("✅ Connected!");
                        break;
                    }
                    Console.Write(".");
                }
            }

            // Verify connection before sending
            if (!await whatsapp.IsConnectedAsync())
            {
                Console.WriteLine("\n❌ WhatsApp is not connected. Please scan the QR code first.");
                return;
            }

            // Send the message
            Console.WriteLine($"\n📤 Sending message to {phoneNumber}...");
            bool success = await whatsapp.SendMessageAsync(phoneNumber, message);

            if (success)
            {
                Console.WriteLine("\n✅ Message sent successfully!");
            }
            else
            {
                Console.WriteLine("\n❌ Failed to send message");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error: {ex.Message}");
        }

        Console.WriteLine("\nDone!");
    }

    static async Task CheckBaileysService()
    {
        Console.WriteLine("=== Baileys Service Check ===\n");

        using var whatsapp = new BaileysWhatsAppService();
        var health = await whatsapp.GetHealthAsync();

        if (health == null)
        {
            Console.WriteLine("❌ Baileys service is NOT running");
            Console.WriteLine("\nTo start the service:");
            Console.WriteLine("1. Open a new terminal");
            Console.WriteLine("2. cd d:\\Projects\\HBDrop\\HBDrop.Baileys");
            Console.WriteLine("3. npm install (first time only)");
            Console.WriteLine("4. npm start");
        }
        else
        {
            Console.WriteLine($"✅ Service is running on http://localhost:3000");
            Console.WriteLine($"   Connected: {health.Connected}");
            Console.WriteLine($"   Needs QR: {health.NeedsQR}");
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine(@"
=== HBDrop - WhatsApp Birthday Automation (Baileys) ===

Usage:
  dotnet run                    - Send a message
  dotnet run -- --check         - Check if Baileys service is running
  dotnet run -- --help          - Show this help

Setup Steps:
1. Start the Baileys service (in a separate terminal):
   cd d:\Projects\HBDrop\HBDrop.Baileys
   npm install
   npm start

2. Scan QR code (first time only)

3. Run this app and enter:
   - Phone number with country code (e.g., +391234567890)
   - Your message

Example:
  Phone: +391234567890
  Message: Testing Baileys! 🚀

Note: The phone number must include the country code with +
");
    }
}
