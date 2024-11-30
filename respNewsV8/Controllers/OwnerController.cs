using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using respNewsV8.Models;

namespace respNewsV8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OwnerController : ControllerBase
    {
        private readonly RespNewContext _sql;
        public OwnerController(RespNewContext sql)
        {
            _sql = sql;
        }
        //GET
        public List<Owner> Get()
        {
            return _sql.Owners.ToList();
        }


        // GET by ID 
        [HttpGet("{id}")]
        public ActionResult<Owner> GetById(int id)
        {
            var owner = _sql.Owners.SingleOrDefault(x => x.OwnerId == id);

            if (owner == null)
            {
                return NotFound();
            }

            return Ok(owner);
        }



        // POST 
        //[Authorize(Roles = "Admin")]

        [HttpPost]
        public IActionResult Post(Owner owner)
        {

            if (owner != null)
            {
                _sql.Owners.Add(owner);
                _sql.SaveChanges();
            }

            return CreatedAtAction(nameof(GetById), new { id = owner.OwnerId }, owner);
        }

        //DELETE
        //[Authorize(Roles = "Admin")]

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var a = _sql.Owners.SingleOrDefault(x => x.OwnerId == id);
            _sql.Owners.Remove(a);
            _sql.SaveChanges();
            return NoContent();
        }
    }
}

