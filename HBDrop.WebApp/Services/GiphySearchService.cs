using System.Text.Json;
using HBDrop.WebApp.Models;

namespace HBDrop.WebApp.Services;

public class GiphySearchService : IGifSearchService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<GiphySearchService> _logger;
    private const string BaseUrl = "https://api.giphy.com/v1/gifs";

    public GiphySearchService(HttpClient httpClient, IConfiguration configuration, ILogger<GiphySearchService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Giphy:ApiKey"] ?? throw new InvalidOperationException("Giphy API key not configured");
        _logger = logger;
    }

    public async Task<GiphySearchResponse?> SearchGifsAsync(string query, int limit = 20, int offset = 0)
    {
        try
        {
            var url = $"{BaseUrl}/search?api_key={_apiKey}&q={Uri.EscapeDataString(query)}&limit={limit}&offset={offset}&rating=g&lang=en";
            _logger.LogInformation("Searching GIFs with query: {Query}", query);
            
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Giphy API search failed with status code: {StatusCode}, Response: {Response}", 
                    response.StatusCode, errorContent);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Giphy API response received, length: {Length}", json.Length);
            
            var result = JsonSerializer.Deserialize<GiphySearchResponse>(json, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
            
            if (result == null)
            {
                _logger.LogError("Failed to deserialize Giphy search response");
                return null;
            }
            
            _logger.LogInformation("Successfully deserialized {Count} GIFs", result.Data?.Count ?? 0);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching GIFs: {Message}", ex.Message);
            return null;
        }
    }

    public async Task<GiphySearchResponse?> GetTrendingGifsAsync(int limit = 20, int offset = 0)
    {
        try
        {
            var url = $"{BaseUrl}/trending?api_key={_apiKey}&limit={limit}&offset={offset}&rating=g";
            _logger.LogInformation("Getting trending GIFs");
            
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Giphy API trending failed with status code: {StatusCode}, Response: {Response}", 
                    response.StatusCode, errorContent);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Giphy trending response received, length: {Length}", json.Length);
            
            var result = JsonSerializer.Deserialize<GiphySearchResponse>(json, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
            
            if (result == null)
            {
                _logger.LogError("Failed to deserialize Giphy response");
                return null;
            }
            
            _logger.LogInformation("Successfully deserialized {Count} trending GIFs", result.Data?.Count ?? 0);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trending GIFs: {Message}", ex.Message);
            return null;
        }
    }
}
