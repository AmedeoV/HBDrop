using System.Text.Json.Serialization;

namespace HBDrop.WebApp.Models;

public class GiphySearchResponse
{
    [JsonPropertyName("data")]
    public List<GiphyGif> Data { get; set; } = new();
    
    [JsonPropertyName("pagination")]
    public GiphyPagination? Pagination { get; set; }
}

public class GiphyGif
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    
    [JsonPropertyName("images")]
    public GiphyImages? Images { get; set; }
}

public class GiphyImages
{
    [JsonPropertyName("original")]
    public GiphyImageData? Original { get; set; }
    
    [JsonPropertyName("fixed_height")]
    public GiphyImageData? FixedHeight { get; set; }
    
    [JsonPropertyName("fixed_height_small")]
    public GiphyImageData? FixedHeightSmall { get; set; }
    
    [JsonPropertyName("preview_gif")]
    public GiphyImageData? PreviewGif { get; set; }
}

public class GiphyImageData
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
    
    [JsonPropertyName("width")]
    public string Width { get; set; } = string.Empty;
    
    [JsonPropertyName("height")]
    public string Height { get; set; } = string.Empty;
}

public class GiphyPagination
{
    [JsonPropertyName("total_count")]
    public int TotalCount { get; set; }
    
    [JsonPropertyName("count")]
    public int Count { get; set; }
    
    [JsonPropertyName("offset")]
    public int Offset { get; set; }
}
