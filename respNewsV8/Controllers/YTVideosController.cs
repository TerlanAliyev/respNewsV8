using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using respNewsV8.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace respNewsV8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class YTVideosController : ControllerBase
    {
        private readonly string _apiKey = "AIzaSyCN3aoUIBNsmhudDFEXiq6KakPyy9mu3h4";
        private readonly string _channelId = "UCjHjGIJFNO93USBxq1pniVw";
        private readonly IDistributedCache _cache;

        public YTVideosController(IDistributedCache cache)
        {
            _cache = cache;
        }

        [HttpGet("{pageNumber}")]
        public async Task<IActionResult> GetVideos(int pageNumber)
        {
            const int maxResultsPerPage = 10; 
            string nextPageToken = null;

            if (pageNumber > 1)
            {
                var tokenCache = await _cache.GetStringAsync($"PageToken_{pageNumber - 1}");
                if (string.IsNullOrEmpty(tokenCache))
                {
                    return BadRequest($"Sayfa {pageNumber - 1} için geçerli bir token bulunamadı.");
                }

                nextPageToken = tokenCache;
            }

            var url = $"https://www.googleapis.com/youtube/v3/search?key={_apiKey}&channelId={_channelId}" +
                      $"&part=snippet&order=date&maxResults={maxResultsPerPage}&pageToken={nextPageToken}";

            using var client = new HttpClient();
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "YouTube API erişim hatası");

            var jsonData = await response.Content.ReadAsStringAsync();
            var videoList = new List<YouTubeVideo>();

            dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonData);
            foreach (var item in data.items)
            {
                videoList.Add(new YouTubeVideo
                {
                    VideoId = item.id.videoId,
                    Title = item.snippet.title,
                    PublishedAt = item.snippet.publishedAt,
                    ThumbnailUrl = item.snippet.thumbnails.@default.url
                });
            }

            string newPageToken = data.nextPageToken;
            if (!string.IsNullOrEmpty(newPageToken))
            {
                await _cache.SetStringAsync($"PageToken_{pageNumber}", newPageToken);
            }

            return Ok(new
            {
                Videos = videoList,
                CurrentPage = pageNumber,
                NextPage = string.IsNullOrEmpty(newPageToken) ? 0 : pageNumber + 1
            });
        }
    }
}
