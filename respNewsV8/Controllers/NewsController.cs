using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using System.IO;
using respNewsV8.Controllers;
using respNewsV8.Models;
using Microsoft.AspNetCore.RateLimiting;



namespace respNewsV8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("FixedWindow")]
    public class NewsController : ControllerBase
    {
        private readonly RespNewContext _sql;


        public NewsController(RespNewContext sql)
        {
            _sql = sql;
        }

        // GET
        [HttpGet]
        public List<News> Get()
        {
            return _sql.News
                .Include(n => n.NewsCategory)
                .Include(n => n.NewsLang)
                .Include(n => n.NewsPhotos)
            .Where(n => n.NewsStatus == true && n.NewsVisibility == true)
            .OrderByDescending(x => x.NewsDate)
            .ThenBy(x => x.NewsRating)
            .Select(n => new News
            {
                NewsId = n.NewsId,
                NewsTitle = n.NewsTitle,
                NewsContetText = n.NewsContetText,
                NewsDate = n.NewsDate,
                NewsCategoryId = n.NewsCategoryId,
                NewsCategory=n.NewsCategory,
                NewsLangId = n.NewsLangId,  
                NewsLang=n.NewsLang,
                NewsVisibility = n.NewsVisibility,
                NewsStatus =n.NewsStatus,
                NewsRating = n.NewsRating,
                NewsUpdateDate=n.NewsUpdateDate,
                NewsViewCount = n.NewsViewCount,
                NewsYoutubeLink=n.NewsYoutubeLink,
                NewsPhotos = n.NewsPhotos,
                NewsVideos=n.NewsVideos

            }).ToList();
        }


        


        // GET by ID
        [HttpGet("{id}")]
        public ActionResult<News> GetById(int id)
        {
            var news = _sql.News
                .Include(n => n.NewsCategory)
                .Include(n => n.NewsLang)
                .Include(n => n.NewsPhotos)
                .Include(n=>n.NewsVideos)
                .SingleOrDefault(x => x.NewsId == id);

            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                WriteIndented = true
            };

            if (news == null)
            {
                return NotFound();
            }
            else
            {
                news.NewsViewCount++;
                _sql.SaveChanges();
            }

            _sql.SaveChanges();

            return new JsonResult(news, options);
            ;
        }



        // Adminler için GET
        //[Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public List<News> GetForAdmins(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _sql.News.Include(n => n.NewsCategory)
                .Include(n => n.NewsLang)
                .Include(n => n.NewsPhotos)
                .Include(n=>n.NewsVideos)
                .AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(n => n.NewsUpdateDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(n => n.NewsUpdateDate <= endDate.Value);
            }

            return query
                .OrderByDescending(x => x.NewsUpdateDate) // Güncellenme tarihine göre sıralama
                .ThenByDescending(x => x.NewsDate) // Öncelikle haber tarihi
                .Select(n => new News
                {
                    NewsId = n.NewsId,
                    NewsTitle = n.NewsTitle,
                    NewsContetText = n.NewsContetText,
                    NewsDate = n.NewsDate,
                    NewsUpdateDate = n.NewsUpdateDate, // Güncellenmiş tarih
                    NewsCategoryId = n.NewsCategoryId,
                    NewsLangId = n.NewsLangId,
                    NewsRating = n.NewsRating,
                    NewsViewCount = n.NewsViewCount,
                    NewsPhotos = n.NewsPhotos,
                    NewsVideos=n.NewsVideos 
                }).ToList();
        }





        // POST
        //[Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UploadNewsDto uploadNewsDto)
        {
            try
            {
                if (uploadNewsDto == null || uploadNewsDto.NewsPhotos == null || uploadNewsDto.NewsPhotos.Count == 0)
                {
                    return BadRequest("Geçersiz veya boş fotoğraflar sağlandı.");
                }

                // Eğer NewsDate null değilse, veritabanına kaydedin
                DateTime newsDate = uploadNewsDto.NewsDate ?? DateTime.Now; // Null değilse kullan, yoksa DateTime.Now kullan

                // Yeni haber kaydını oluştur
                News news = new News
                {
                    NewsTitle = uploadNewsDto.NewsTitle,
                    NewsContetText = uploadNewsDto.NewsContetText,
                    NewsYoutubeLink = uploadNewsDto.NewsYoutubeLink,
                    NewsCategoryId = uploadNewsDto.NewsCategoryId,
                    NewsLangId = uploadNewsDto.NewsLangId,
                    NewsRating = uploadNewsDto.NewsRating,
                    NewsDate = newsDate,  // Burada doğru DateTime'ı kaydediyoruz
                    NewsUpdateDate = DateTime.Now,
                    NewsStatus = true,
                    NewsVisibility = true
                };

                // Haber kaydını veritabanına ekle
                _sql.News.Add(news);
                await _sql.SaveChangesAsync();

                if (uploadNewsDto.NewsPhotos != null)
                {
                    foreach (var photoUrl in uploadNewsDto.NewsPhotos)
                    {
                        NewsPhoto newsPhoto = new NewsPhoto
                        {
                            PhotoUrl = photoUrl,
                            PhotoNewsId = news.NewsId
                        };
                        _sql.NewsPhotos.Add(newsPhoto);
                    }
                }

                if (uploadNewsDto.NewsVideos != null)
                {
                    foreach (var video in uploadNewsDto.NewsVideos)
                    {
                        var videoUrl = await SaveFileAsync(video);

                        NewsVideo newsVideo = new NewsVideo
                        {
                            VideoUrl = videoUrl,
                            VideoNewsId = news.NewsId
                        };
                        _sql.NewsVideos.Add(newsVideo);
                    }
                }




                // Fotoğrafları ve haber bilgilerini veritabanına kaydet
                await _sql.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = news.NewsId }, news);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server hatası: {ex.Message}");
            }
        
        }


        // Video dosyalarını belirli bir dizine kaydeder
        private async Task<string> SaveFileAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "NewsVideos");
            Directory.CreateDirectory(uploadsFolder); // Klasör yoksa oluştur
            var filePath = Path.Combine(uploadsFolder, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Dosya URL'sini döndür
            return $"/NewsVideos/{file.FileName}";
        }




        // DELETE
        //[Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var news = _sql.News.SingleOrDefault(x => x.NewsId == id);
            if (news == null)
            {
                return NotFound();
            }

            news.NewsStatus = false;
            news.NewsVisibility = false;
            _sql.SaveChanges();
            return NoContent();
        }

        // EDIT
        //[Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult Put(int id, News news)
        {
            var old = _sql.News.SingleOrDefault(x => x.NewsId == id);
            if (old == null)
            {
                return NotFound();
            }
            old.NewsStatus = news.NewsStatus;
            old.NewsVisibility = news.NewsVisibility;
            old.NewsTitle = news.NewsTitle;
            old.NewsContetText = news.NewsContetText;
            old.NewsDate = old.NewsDate;
            old.NewsCategoryId = news.NewsCategoryId;
            old.NewsLangId = news.NewsLangId;
            old.NewsRating = news.NewsRating;
            old.NewsUpdateDate = DateTime.Now;


            _sql.SaveChanges();

            return NoContent();
        }
        // EDIT (visibility Update )
        //[Authorize(Roles = "Admin")]
        [HttpPut("{id}/visibility")]
        public IActionResult UpdateVisibility(int id, [FromBody] UpdateVisibilityDto visibilityDto)
        {
            var news = _sql.News.SingleOrDefault(x => x.NewsId == id);
            if (news == null)
            {
                return NotFound();
            }

            news.NewsVisibility = visibilityDto.IsVisible;
            _sql.SaveChanges();

            return NoContent();
        }

        // DTO for visibility update
        public class UpdateVisibilityDto
        { 
            public bool IsVisible { get; set; }
        }

    }
}