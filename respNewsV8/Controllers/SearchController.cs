using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using respNewsV8.Models;
using respNewsV8.Services;

namespace respNewsV8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly RespNewContext _sql;
        public SearchController(RespNewContext sql)
        {
            _sql = sql;

        }

        [HttpGet("{query}")]
        public async Task<IActionResult> GetById(string query)
        {
            string word = query;
            var results = await _sql.News
                .Where(x =>
                    x.NewsTitle.Contains(word) ||
                    x.NewsContetText.Contains(word) ||
                    x.NewsCategory.CategoryName.Contains(word) ||
                    x.NewsOwner.OwnerName.Contains(word) ||
                    x.NewsTags.Any(tag => tag.TagName.Contains(word)) // TagName içinde arama
                )
                .Select(n => new News
                {
                    NewsId = n.NewsId,
                    NewsTitle = n.NewsTitle,  /*+++*/
                    NewsContetText = n.NewsContetText,  /*+++*/
                    NewsDate = n.NewsDate,
                    NewsCategoryId = n.NewsCategoryId,
                    NewsCategory = n.NewsCategory,  /*+++*/
                    NewsLangId = n.NewsLangId,
                    NewsLang = n.NewsLang,
                    NewsVisibility = n.NewsVisibility,
                    NewsStatus = n.NewsStatus,
                    NewsRating = n.NewsRating,
                    NewsUpdateDate = n.NewsUpdateDate,
                    NewsViewCount = n.NewsViewCount,
                    NewsYoutubeLink = n.NewsYoutubeLink,
                    NewsPhotos = n.NewsPhotos,
                    NewsVideos = n.NewsVideos,
                    NewsTags = n.NewsTags, /*+++*/
                    NewsOwner = n.NewsOwner /*+++*/
                })
                .ToListAsync();

            return Ok(results);
        }

    }
}
