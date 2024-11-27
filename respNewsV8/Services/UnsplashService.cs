using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace respNewsV8.Services
{
	public class UnsplashService
	{
		private readonly HttpClient _httpClient;
		private readonly string _accessKey;

		public UnsplashService(HttpClient httpClient, IConfiguration configuration)
		{
			_httpClient = httpClient;
			_accessKey = configuration["Unsplash:AccessKey"];
		}

		public async Task<List<string>> SearchImageAsync(string query)
		{
			try
			{
				var url = $"https://api.unsplash.com/search/photos?query={query}&client_id={_accessKey}";

				var response = await _httpClient.GetAsync(url);
				if (!response.IsSuccessStatusCode)
				{
					var errorContent = await response.Content.ReadAsStringAsync();
					throw new Exception($"Unsplash API error: {errorContent}");
				}

				var jsonResponse = await response.Content.ReadAsStringAsync();

				var jObject = JsonConvert.DeserializeObject<JObject>(jsonResponse);
				var results = jObject.SelectToken("results");

				if (results == null)
				{

					throw new Exception("The 'results' array was not found in the API response");
				}

				// URL'leri almaq ucun
				var photoUrls = results.Select(x => x["urls"]?["regular"]?.ToString()).Where(url => !string.IsNullOrEmpty(url)).ToList();

				return photoUrls;
			}
			catch (Exception ex)
			{
				// loglama
				throw new Exception("An error occurred during the Unsplash API call.", ex);
			}
		}
	}
}
