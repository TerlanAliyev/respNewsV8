public class GeoLocationService
{
    private readonly HttpClient _httpClient;

    public GeoLocationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetLocationFromIP(string ipAddress)
    {
        // IP adresine göre konum bilgisini döndürme
        var response = await _httpClient.GetAsync($"https://ip-geolocation-api.com/{ipAddress}");
        return await response.Content.ReadAsStringAsync();
    }
}
