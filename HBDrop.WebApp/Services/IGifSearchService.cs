namespace HBDrop.WebApp.Services;

public interface IGifSearchService
{
    Task<Models.GiphySearchResponse?> SearchGifsAsync(string query, int limit = 20, int offset = 0);
    Task<Models.GiphySearchResponse?> GetTrendingGifsAsync(int limit = 20, int offset = 0);
}
