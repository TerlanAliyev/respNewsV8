using Microsoft.AspNetCore.Http.HttpResults;
using Newtonsoft.Json;

public class GeoLocationService
{
    private readonly HttpClient _httpClient;

    // HttpClient'ı DI yoluyla alıyoruz
    public GeoLocationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GeoLocationModel> GetLocationFromIP(string ipAddress)
    {
        string apiKey = "f360383cc9aed4e04dc54e4d677bf92f";  // API anahtarınızı buraya yerleştirin
        string url = $"http://api.ipstack.com/{ipAddress}?access_key={apiKey}";

        try
        {
            // API'den gelen yanıtı alıyoruz
            var response = await _httpClient.GetStringAsync(url);

            // JSON yanıtını GeoLocationModel türüne deserialize ediyoruz
            var locationData = JsonConvert.DeserializeObject<GeoLocationModel>(response);

            return locationData;
        }
        catch (Exception ex)
        {
            // Hata oluşursa null döndürüyoruz ve hatayı logluyoruz
            Console.WriteLine($"GeoLocation Error: {ex.Message}");
            return null;
        }
    }
}





