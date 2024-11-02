using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        //GET
        public List<Category> Get()
        {
            return _sql.Categories.ToList();
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
        [Authorize(Roles = "Admin")]

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

