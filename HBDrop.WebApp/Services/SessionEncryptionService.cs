using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace HBDrop.WebApp.Services;

/// <summary>
/// Service for encrypting and decrypting WhatsApp session data
/// Uses AES-256 encryption with a user-specific key derived from user ID + app secret
/// </summary>
public class SessionEncryptionService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SessionEncryptionService> _logger;

    public SessionEncryptionService(IConfiguration configuration, ILogger<SessionEncryptionService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Encrypt session data for storage in database
    /// </summary>
    /// <param name="sessionData">Raw session data object</param>
    /// <param name="userId">User ID for key derivation</param>
    /// <returns>Tuple of (encryptedData, iv)</returns>
    public (string EncryptedData, string IV) EncryptSessionData(object sessionData, string userId)
    {
        try
        {
            var jsonData = JsonSerializer.Serialize(sessionData);
            var plainBytes = Encoding.UTF8.GetBytes(jsonData);

            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Key = DeriveKey(userId);
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                cs.Write(plainBytes, 0, plainBytes.Length);
            }

            var encryptedData = Convert.ToBase64String(ms.ToArray());
            var iv = Convert.ToBase64String(aes.IV);

            _logger.LogDebug("Session data encrypted for user {UserId}", userId);
            return (encryptedData, iv);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting session data for user {UserId}", userId);
            throw new InvalidOperationException("Failed to encrypt session data", ex);
        }
    }

    /// <summary>
    /// Decrypt session data from database
    /// </summary>
    /// <param name="encryptedData">Base64 encoded encrypted data</param>
    /// <param name="iv">Base64 encoded initialization vector</param>
    /// <param name="userId">User ID for key derivation</param>
    /// <returns>Decrypted session data as JSON string</returns>
    public string DecryptSessionData(string encryptedData, string iv, string userId)
    {
        try
        {
            var encryptedBytes = Convert.FromBase64String(encryptedData);
            var ivBytes = Convert.FromBase64String(iv);

            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Key = DeriveKey(userId);
            aes.IV = ivBytes;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(encryptedBytes);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            
            var decryptedData = sr.ReadToEnd();
            
            _logger.LogDebug("Session data decrypted for user {UserId}", userId);
            return decryptedData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting session data for user {UserId}", userId);
            throw new InvalidOperationException("Failed to decrypt session data", ex);
        }
    }

    /// <summary>
    /// Derive a user-specific encryption key using PBKDF2
    /// Combines user ID with application secret for key derivation
    /// </summary>
    private byte[] DeriveKey(string userId)
    {
        // Get the master encryption key from configuration
        var masterKey = _configuration["Encryption:MasterKey"] 
            ?? throw new InvalidOperationException("Encryption master key not configured");

        // Combine user ID with master key for user-specific salt
        var salt = Encoding.UTF8.GetBytes($"{userId}:{masterKey}");

        // Use PBKDF2 to derive a 256-bit key
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password: masterKey,
            salt: salt,
            iterations: 10000,
            hashAlgorithm: HashAlgorithmName.SHA256);

        return pbkdf2.GetBytes(32); // 256 bits = 32 bytes
    }

    /// <summary>
    /// Generate a secure random master key (for initial setup)
    /// </summary>
    public static string GenerateMasterKey()
    {
        using var rng = RandomNumberGenerator.Create();
        var keyBytes = new byte[32]; // 256 bits
        rng.GetBytes(keyBytes);
        return Convert.ToBase64String(keyBytes);
    }
}
