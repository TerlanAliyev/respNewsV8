using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using respNewsV8.Models;
using respNewsV8.Services;

namespace respNewsV8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SelectDateController : ControllerBase
    {
        private readonly RespNewContext _sql;
        public SelectDateController(RespNewContext sql)
        {
            _sql = sql;
        }


        [HttpGet("{date}")]
        public async Task<IActionResult> GetById(string date)
        {
            // Gelen string tarihi sadece "yyyy-MM-dd" formatında parse et
            DateTime parsedDate = DateTime.ParseExact(date, "yyyy-MM-dd", null);

            // NewsDate'in sadece tarih kısmını karşılaştır
            var results = await _sql.News
                .Where(x => x.NewsDate >= parsedDate && x.NewsDate < parsedDate.AddDays(1)) // Tarihi, bir sonraki güne kadar al
                .Select(n => new News
                {
                    NewsId = n.NewsId,
                    NewsTitle = n.NewsTitle,
                    NewsContetText = n.NewsContetText,
                    NewsDate = n.NewsDate,
                    NewsCategoryId = n.NewsCategoryId,
                    NewsCategory = n.NewsCategory,
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
                    NewsTags = n.NewsTags,
                    NewsOwner = n.NewsOwner
                })
                .ToListAsync();

            return Ok(results);
        }



    }
}
