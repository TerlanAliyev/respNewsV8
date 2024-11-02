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



namespace respNewsV8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
            return _sql.News.Include(n => n.NewsCategory).Include(n => n.NewsLang).Include(n => n.NewsPhotos)
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
                NewsLangId = n.NewsLangId,
                NewsRating = n.NewsRating,
                NewsViewCount = n.NewsViewCount,
                NewsPhotos = n.NewsPhotos,
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
        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public List<News> GetForAdmins(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _sql.News.Include(n => n.NewsCategory)
                .Include(n => n.NewsLang)
                .Include(n => n.NewsPhotos)

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
                }).ToList();
        }





        // POST
        //[Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Post([FromForm] UploadNewsDto uploadNewsDto)
        {
            try
            {
                if (uploadNewsDto == null || uploadNewsDto.NewsPhotos == null || uploadNewsDto.NewsPhotos.Count == 0)
                {
                    return BadRequest("Useless or no photos provided.");
                }

                News news = new News
                {
                    NewsTitle = uploadNewsDto.NewsTitle,
                    NewsContetText = uploadNewsDto.NewsContetText,
                    NewsCategoryId = uploadNewsDto.NewsCategoryId,
                    NewsLangId = uploadNewsDto.NewsLangId,
                    NewsRating = uploadNewsDto.NewsRating,
                    NewsDate = DateTime.Now,
                    NewsUpdateDate = DateTime.Now,
                    NewsStatus = true,
                    NewsVisibility = true
                };

                _sql.News.Add(news);
                await _sql.SaveChangesAsync(); // Değişiklikleri kaydet

                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "NewsImages");

                // Eğer dizin yoksa oluştur
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                foreach (var photo in uploadNewsDto.NewsPhotos)
                {
                    // Benzersiz dosya adı oluştur
                    string fileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(photo.FileName);
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    // Asenkron olarak fotoğrafı kaydet
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await photo.CopyToAsync(stream); // Fotoğrafı kaydet
                    }

                    NewsPhoto newsPhoto = new NewsPhoto
                    {
                        PhotoUrl = $"/images/uploads/{fileName}", // Kaydedilen fotoğrafın URL'si
                        PhotoNewsId = news.NewsId
                    };

                    _sql.NewsPhotos.Add(newsPhoto);
                }

                await _sql.SaveChangesAsync(); // Değişiklikleri kaydet

                return CreatedAtAction(nameof(GetById), new { id = news.NewsId }, news);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server xetası: {ex.Message}");
            }
        }



        // DELETE
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var news = _sql.News.SingleOrDefault(x => x.NewsId == id);
            if (news == null)
            {
                return NotFound();
            }

            news.NewsStatus = false;
            _sql.SaveChanges();
            return NoContent();
        }

        // EDIT
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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