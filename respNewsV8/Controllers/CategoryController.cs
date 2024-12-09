using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using respNewsV8.Models;

namespace respNewsV8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly RespNewContext _sql;
        public CategoryController(RespNewContext sql)
        {
            _sql = sql;
        }

        private int? GetLanguageIdByCode(int langCode)
        {
            var language = _sql.Languages.FirstOrDefault(l => l.LanguageId == langCode);
            if (language == null)
            {
                Console.WriteLine($"Dil kodu bulunamadı: {langCode}");
                return null;
            }
            return language.LanguageId;
        }

        //GET
        [HttpGet("language/{langId}")]
        public List<Category> Get(int langId)
        {
            var lang = langId;
            return _sql.Categories.Where(x=>x.CategoryLangId==langId).ToList();
        }

        //umumi
        [HttpGet("count")]
        public IActionResult GetCategoryCount()
        {
                // Kategorilerin sayısını almak
                var categoryCount = _sql.Categories
                    .Select(x => x.CategoryName)  // Kategori ismi
                    .Distinct()                   // Benzersiz kategoriler
                    .Count();                     // Sayma işlemi

                return Ok(new { categoryCount });  // JSON formatında sayıyı döndürme
            
            
        }

        //GET misal 2dene az dilinde -https://localhost:44314/api/category/count/1
        [HttpGet("count/{langId}")]
        public IActionResult GetCategoryCountByLang(int langId)
        {
            try
            {
                // Kategorilerin sayısını almak
                var categoryCount = _sql.Categories
                    .Where(x => x.CategoryLangId == langId) // Dil ID'ye göre filtreleme
                    .Select(x => x.CategoryName)  // Kategori ismi
                    .Distinct()                   // Benzersiz kategoriler
                    .Count();                     // Sayma işlemi

                return Ok(new { categoryCount });  // JSON formatında sayıyı döndürme
            }
            catch (Exception ex)
            {
                // Hata durumunda uygun bir mesaj döndürme
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }


        // GET by ID 
        [HttpGet("{id}")]
        public ActionResult<Category> GetById(int id)
        {
            var category = _sql.Categories.SingleOrDefault(x => x.CategoryId == id);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }



        // POST 
        //[Authorize(Roles = "Admin")]

        [HttpPost]
        public IActionResult Post(Category category)
        {

            if (category == null)
            {
                return BadRequest("Geçersiz kategori verisi.");
            }

            _sql.Categories.Add(category);
            _sql.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = category.CategoryId }, category);
        }

        //DELETE
        //[Authorize(Roles = "Admin")]

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var a = _sql.Categories.SingleOrDefault(x => x.CategoryId == id);
            _sql.Categories.Remove(a);
            _sql.SaveChanges();
            return NoContent();
        }
    }
}

