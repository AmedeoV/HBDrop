using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HBDrop.WebApp.Services;

public class AIMessageService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AIMessageService> _logger;
    private readonly string _endpoint;
    private readonly string _model;
    private readonly int _timeout;

    public AIMessageService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<AIMessageService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        
        _endpoint = configuration["AI:OllamaEndpoint"] ?? "http://localhost:11434";
        _model = configuration["AI:Model"] ?? "llama3.2:3b";
        _timeout = configuration.GetValue<int>("AI:Timeout", 30);
        
        _httpClient.Timeout = TimeSpan.FromSeconds(_timeout);
    }

    public async Task<string> GenerateMessageAsync(
        string contactName,
        string eventType,
        string? notes = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = BuildPrompt(contactName, eventType, notes);
            
            var request = new OllamaGenerateRequest
            {
                Model = _model,
                Prompt = prompt,
                Stream = false,
                Options = new OllamaOptions
                {
                    Temperature = 0.7,
                    MaxTokens = 150
                }
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"{_endpoint}/api/generate",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Ollama API returned error: {StatusCode}", response.StatusCode);
                return GenerateFallbackMessage(contactName, eventType);
            }

            var result = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(cancellationToken);
            
            if (result?.Response == null)
            {
                _logger.LogWarning("Ollama returned empty response");
                return GenerateFallbackMessage(contactName, eventType);
            }

            // Clean up the response - remove quotes and trim
            var cleanedMessage = result.Response.Trim();
            
            // Remove surrounding quotes (both straight and curly quotes)
            if ((cleanedMessage.StartsWith("\"") && cleanedMessage.EndsWith("\"")) ||
                (cleanedMessage.StartsWith("'") && cleanedMessage.EndsWith("'")))
            {
                cleanedMessage = cleanedMessage.Substring(1, cleanedMessage.Length - 2);
            }
            
            return cleanedMessage.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI message");
            return GenerateFallbackMessage(contactName, eventType);
        }
    }

    private string BuildPrompt(string contactName, string eventType, string? notes)
    {
        var prompt = new StringBuilder();
        prompt.AppendLine($"Generate a warm, personalized {eventType} message for {contactName}.");
        
        if (!string.IsNullOrWhiteSpace(notes))
        {
            prompt.AppendLine($"Consider these personal details: {notes}");
        }
        
        prompt.AppendLine("Requirements:");
        prompt.AppendLine("- Keep it friendly and genuine");
        prompt.AppendLine("- 2-3 sentences maximum");
        prompt.AppendLine("- Don't use generic phrases");
        prompt.AppendLine("- Make it feel personal");
        prompt.AppendLine();
        prompt.AppendLine("Generate only the message text, no quotes or explanations:");

        return prompt.ToString();
    }

    private string GenerateFallbackMessage(string contactName, string eventType)
    {
        return eventType.ToLower() switch
        {
            "birthday" => $"Happy Birthday {contactName}! 🎉 Wishing you an amazing day filled with joy and happiness!",
            "anniversary" => $"Happy Anniversary {contactName}! 💕 Celebrating this special milestone with you!",
            _ => $"Happy {eventType} {contactName}! 🎊 Hope your day is wonderful!"
        };
    }

    public async Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_endpoint}/api/tags", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> EnsureModelDownloadedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Checking if model {Model} is available", _model);
            
            var pullRequest = new OllamaPullRequest
            {
                Name = _model,
                Stream = false
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"{_endpoint}/api/pull",
                pullRequest,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Model {Model} is ready", _model);
                return true;
            }

            _logger.LogWarning("Failed to ensure model {Model} is available", _model);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking model availability");
            return false;
        }
    }
}

// Ollama API Models
public class OllamaGenerateRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }

    [JsonPropertyName("options")]
    public OllamaOptions? Options { get; set; }
}

public class OllamaOptions
{
    [JsonPropertyName("temperature")]
    public double Temperature { get; set; }

    [JsonPropertyName("num_predict")]
    public int MaxTokens { get; set; }
}

public class OllamaGenerateResponse
{
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("response")]
    public string? Response { get; set; }

    [JsonPropertyName("done")]
    public bool Done { get; set; }
}

public class OllamaPullRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }
}
