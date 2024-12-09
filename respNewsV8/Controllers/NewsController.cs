﻿using Microsoft.AspNetCore.Http;
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
using respNewsV8.Services;



namespace respNewsV8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly RespNewContext _sql;
		private readonly UnsplashService _unsplashService;


		public NewsController(RespNewContext sql, UnsplashService unsplashService)
        {
            _sql = sql;
			_unsplashService = unsplashService;

		}
        private int? GetLanguageIdByCode(int langCode,int categoryId)
        {
            var language = _sql.Languages.FirstOrDefault(l => l.LanguageId == langCode);
            var category = _sql.Categories.FirstOrDefault(x => x.CategoryId == categoryId);
            if (language == null)
            {
                Console.WriteLine($"Dil kodu bulunamadı: {langCode}");
                return null; 
            }
            if (category == null)
            {
                Console.WriteLine($"Category kodu bulunamadı: {categoryId}");
                return null;
            }
            return language.LanguageId;
        }






        //umumi
        [HttpGet("count")]
        public IActionResult GetNewsCount()
        {
            // Kategorilerin sayısını almak
            var NewsCount = _sql.News
                .Select(x => x.NewsId)  // Kategori ismi
                .Distinct()                   // Benzersiz kategoriler
                .Count();                     // Sayma işlemi

            return Ok(new { NewsCount });  // JSON formatında sayıyı döndürme


        }

        //GET misal 2dene az dilinde -https://localhost:44314/api/category/count/1
        [HttpGet("count/{langId}")]
        public IActionResult GetNewsCountByLang(int langId)
        {
            try
            {
                // Kategorilerin sayısını almak
                var NewsCount = _sql.News
                    .Where(x => x.NewsLangId == langId) // Dil ID'ye göre filtreleme
                    .Select(x => x.NewsId)  // Kategori ismi
                    .Distinct()                   // Benzersiz kategoriler
                    .Count();                     // Sayma işlemi

                return Ok(new { NewsCount });  // JSON formatında sayıyı döndürme
            }
            catch (Exception ex)
            {
                // Hata durumunda uygun bir mesaj döndürme
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        //umumi sekiller
        [HttpGet("photos/count")]
        public IActionResult GetNewsPhotosCount()
        {
            // Kategorilerin sayısını almak
            var NewsCount = _sql.NewsPhotos
                .Select(x => x.PhotoId)  // Kategori ismi
                .Distinct()                   // Benzersiz kategoriler
                .Count();                     // Sayma işlemi

            return Ok(new { NewsCount });  // JSON formatında sayıyı döndürme


        }




        [HttpGet("language/{langCode}/{pageNumber}")]
        public IActionResult Get(int langCode,int pageNumber)
        {
            int languageId = langCode;
            int page = pageNumber;
            if (languageId == null)
            {
                return NotFound($"Dil kodu '{langCode}' için bir ID bulunamadı.");
            }

            var newsList = _sql.News
                .Include(n => n.NewsCategory)
                .Include(n => n.NewsLang)
                .Include(n => n.NewsPhotos)
                .Include(n => n.NewsTags)
                .Include(n => n.NewsOwner)
                .Include(n=>n.NewsTags)
                .Where(n => n.NewsStatus == true && n.NewsVisibility == true)
                .Where(n => n.NewsLangId == languageId) 
                .OrderByDescending(x => x.NewsDate)
                .ThenBy(x => x.NewsRating)
                .Select(n => new
                {
                    n.NewsId,
                    n.NewsTitle,
                    n.NewsContetText,
                    n.NewsDate,
                    n.NewsCategoryId,
                    n.NewsCategory,
                    n.NewsLangId,
                    n.NewsLang,
                    n.NewsVisibility,
                    n.NewsStatus,
                    n.NewsRating,
                    n.NewsOwner,
                    n.NewsUpdateDate,
                    n.NewsViewCount,
                    n.NewsYoutubeLink,
                    n.NewsTags,
                    n.NewsPhotos,
                    n.NewsVideos
                }).Skip(page * 3).Take(3).ToList();

            if (!newsList.Any())
            {
                return NotFound($"Dil kodu '{langCode}' için uygun haber bulunamadı.");
            }

            return Ok(newsList);
        }


        [HttpGet("rating/{RatingCode}/{langCode}")]
        public IActionResult GetByRating(int RatingCode,int langCode)
        {
            int RatingId = RatingCode;
            int languageId = langCode;

            if (RatingId == null)
            {
                return NotFound($"Rating kodu '{RatingId}' için bir kod bulunamadı.");
            }
            if (languageId == null)
            {
                return NotFound($"Dil kodu '{languageId}' için bir kod bulunamadı.");
            }
            var newsList = _sql.News
                .Include(n => n.NewsCategory)
                .Include(n => n.NewsLang)
                .Include(n => n.NewsPhotos)
                .Include(n => n.NewsTags)
                .Include(n => n.NewsOwner)
                .Include(n=>n.NewsTags)
                .Where(n => n.NewsStatus == true && n.NewsVisibility == true)
                .Where(n => n.NewsRating == RatingId)
                .Where(n => n.NewsLangId == languageId)
                .OrderByDescending(x => x.NewsDate)
                .Select(n => new
                {
                    n.NewsId,
                    n.NewsTitle,
                    n.NewsContetText,
                    n.NewsDate,
                    n.NewsCategoryId,
                    n.NewsCategory,
                    n.NewsLangId,
                    n.NewsLang,
                    n.NewsVisibility,
                    n.NewsStatus,
                    n.NewsRating,
                    n.NewsOwner,
                    n.NewsUpdateDate,
                    n.NewsViewCount,
                    n.NewsYoutubeLink,
                    n.NewsTags,
                    n.NewsPhotos,
                    n.NewsVideos
                }).ToList();

            if (!newsList.Any())
            {
                return NotFound($"Dil kodu '{RatingCode}' için uygun haber bulunamadı.");
            }

            return Ok(newsList);
        }



        //Categoryler ucun
        [HttpGet("language/{langCode}/{categoryId}/{pageNumber}")]
        public IActionResult Get(int langCode, int pageNumber,int categoryId)
        {
            int languageId = langCode;
            int category = categoryId;
            int page = pageNumber;
            if (languageId == null)
            {
                return NotFound($"Dil kodu '{langCode}' için bir ID bulunamadı.");
            }

            var newsList = _sql.News
                .Include(n => n.NewsCategory)
                .Include(n => n.NewsLang)
                .Include(n => n.NewsPhotos)
                .Include(n => n.NewsTags)
                .Include(n => n.NewsOwner)
                .Include(n=>n.NewsTags)
                .Where(n => n.NewsStatus == true && n.NewsVisibility == true)
                .Where(n => n.NewsLangId == languageId)
                .Where(n=>n.NewsCategoryId==categoryId)
                .OrderByDescending(x => x.NewsDate)
                .ThenBy(x => x.NewsRating)
                .Select(n => new
                {
                    n.NewsId,
                    n.NewsTitle,
                    n.NewsContetText,
                    n.NewsDate,
                    n.NewsCategoryId,
                    n.NewsCategory,
                    n.NewsLangId,
                    n.NewsLang,
                    n.NewsVisibility,
                    n.NewsStatus,
                    n.NewsRating,
                    n.NewsOwner,
                    n.NewsUpdateDate,
                    n.NewsViewCount,
                    n.NewsYoutubeLink,
                    n.NewsTags,
                    n.NewsPhotos,
                    n.NewsVideos
                }).Skip(page * 3).Take(3).ToList();

            if (!newsList.Any())
            {
                return NotFound($"Dil kodu '{langCode}' için uygun haber bulunamadı.");
            }

            return Ok(newsList);
        }





        // GET by ID
        [HttpGet("id/{id}")]
        public ActionResult<News> GetById(int id)
        {
            var news = _sql.News
                .Include(n => n.NewsCategory)
                .Include(n => n.NewsLang)
                .Include(n => n.NewsPhotos)
                .Include(n=>n.NewsVideos)
                .Include(n => n.NewsTags)
                .Include(n => n.NewsOwner)
                .Include(n=>n.NewsTags)
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



		// Adminler ucun GET
		//[Authorize(Roles = "Admin")]
		[HttpGet("admin/{pageNumber}")]
        public List<News> GetForAdmins(DateTime? startDate = null, DateTime? endDate = null,int pageNumber=0)
        {
            int page = pageNumber;

            var query = _sql.News.Include(n => n.NewsCategory)
                .Include(n => n.NewsLang)
                .Include(n => n.NewsPhotos)
                .Include(n => n.NewsOwner)
                .Include(n=>n.NewsTags)
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
                .OrderByDescending(x => x.NewsUpdateDate)
                .ThenByDescending(x => x.NewsDate) 
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
                  NewsTags=n.NewsTags, /*+++*/
                  NewsOwner =n.NewsOwner /*+++*/

              }).Skip(page*2).Take(2).ToList();
        }



        [HttpGet("slider")]
        public List<News> Slider(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _sql.News.Include(n => n.NewsCategory)
                .Include(n => n.NewsLang)
                .Include(n => n.NewsPhotos)
                .Include(n=>n.NewsOwner)
                .Include(n=>n.NewsTags)
                .Include(n => n.NewsVideos)
                .AsQueryable();

            // Sadece ratingi 5 olan xeberler
            query = query.Where(n => n.NewsRating == 5);

            // Tarix aralığına göre filtr
            if (startDate.HasValue)
            {
                query = query.Where(n => n.NewsDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(n => n.NewsDate <= endDate.Value);
            }

            // Sıralama: Güncellenme tarixine göre azalan sıralama
            return query
                .OrderByDescending(n => n.NewsDate)
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
                    NewsOwner=n.NewsOwner,
                    NewsTags=n.NewsTags
                })
                .ToList();
        }


        //UNSPLASH
        [HttpGet("search")]
        public async Task<IActionResult> SearchPhotos(string query) 
        {
            try
            {
                var photoUrls = await _unsplashService.SearchImageAsync(query);
                return Ok(photoUrls);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }


        // POST
        //[Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Post([FromForm] UploadNewsDto uploadNewsDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Geçersiz model.");
            }
            try
            {
                DateTime newsDate = uploadNewsDto.NewsDate ?? DateTime.Now;

                News news = new News
                {
                    NewsTitle = uploadNewsDto.NewsTitle,
                    NewsContetText = uploadNewsDto.NewsContetText,
                    NewsYoutubeLink = uploadNewsDto.NewsYoutubeLink,
                    NewsCategoryId = uploadNewsDto.NewsCategoryId,
                    NewsLangId = uploadNewsDto.NewsLangId,
                    NewsOwnerId = uploadNewsDto.NewsOwnerId,
                    NewsRating = uploadNewsDto.NewsRating,
                    NewsDate = newsDate,
                    NewsUpdateDate = DateTime.Now,
                    NewsStatus = true,
                    NewsVisibility = true
                };
               
                _sql.News.Add(news);
                await _sql.SaveChangesAsync();

                if (uploadNewsDto.NewsPhotos != null)
                {
                    foreach (var photo in uploadNewsDto.NewsPhotos)
                    {
                        var PhotoUrl = await SaveFileAsync(photo, "NewsPhotos"); // Foto kaydetme metodu çağrılıyor

                        NewsPhoto newsPhoto = new NewsPhoto
                        {
                            PhotoUrl = PhotoUrl,
                            PhotoNewsId = news.NewsId
                        };
                        _sql.NewsPhotos.Add(newsPhoto);
                    }
                }

                if (uploadNewsDto.Tags != null)
                {
                    foreach (var tagName in uploadNewsDto.Tags)
                    {
                        var tag = new NewsTag
                        {
                            TagName = tagName,
                            TagNewsId = news.NewsId // Eklenen haberin ID'sini al
                        };

                         _sql.NewsTags.AddAsync(tag);
                    }

                    _sql.SaveChangesAsync(); // Tüm etiketleri kaydet
                }


                // Videoları kaydetme
                if (uploadNewsDto.NewsVideos != null)
                {
                    foreach (var video in uploadNewsDto.NewsVideos)
                    {
                        var videoUrl = await SaveFileAsync(video, "NewsVideos"); // Video kaydetme metodu çağrılıyor

                        NewsVideo newsVideo = new NewsVideo
                        {
                            VideoUrl = videoUrl,
                            VideoNewsId = news.NewsId
                        };
                        _sql.NewsVideos.Add(newsVideo);
                    }
                }

                await _sql.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = news.NewsId }, news);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server hatası: {ex.Message}");
            }
        }



        private async Task<string> SaveFileAsync(IFormFile file, string folderName)
        {
            try
            {
                // Dosya uzantısını al
                var fileExtension = Path.GetExtension(file.FileName);

                // Rastgele bir GUID oluştur ve dosya uzantısıyla birleştir
                var fileName = Guid.NewGuid().ToString() + fileExtension;

                // Yükleme klasörünü oluştur
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderName);
                Directory.CreateDirectory(uploadsFolder);

                // Dosya yolu
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Dosyayı kaydet
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Kaydedilen dosyanın URL'sini döndür
                return $"/{folderName}/{fileName}";
            }
            catch (Exception ex)
            {
                throw new Exception("Dosya kaydedilirken bir hata oluştu: " + ex.Message);
            }
        }




        // DELETE
        //[Authorize(Roles = "Admin")]
        [HttpDelete("id/{id}")]
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
            old.NewsOwnerId = news.NewsOwnerId;
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
            news.NewsUpdateDate = DateTime.Now;
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