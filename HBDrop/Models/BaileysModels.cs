using System.Text.Json.Serialization;

namespace HBDrop.Models;

public class HealthResponse
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    
    [JsonPropertyName("connected")]
    public bool Connected { get; set; }
    
    [JsonPropertyName("needsQR")]
    public bool NeedsQR { get; set; }
}

public class QrResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    
    [JsonPropertyName("qrCode")]
    public string? QrCode { get; set; }
    
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public class SendResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    
    [JsonPropertyName("message")]
    public string? Message { get; set; }
    
    [JsonPropertyName("to")]
    public string? To { get; set; }
    
    [JsonPropertyName("error")]
    public string? Error { get; set; }
}
