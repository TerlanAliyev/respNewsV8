using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using respNewsV8.Models;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace respNewsV8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {

        private readonly RespNewContext _sql;

        public MessagesController(RespNewContext sql)
        {
            _sql = sql;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var messages = await _sql.Messagesses.ToListAsync();
            return Ok(messages);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var messages = await _sql.Messagesses.Where(x => x.MessageId == id).ToListAsync();
            return Ok(messages);
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Messagess messagess)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            messagess.MessageDate = DateTime.Now;
            await _sql.Messagesses.AddAsync(messagess);
            await _sql.SaveChangesAsync();

            return Ok();
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var message = _sql.Messagesses.SingleOrDefault(x=>x.MessageId==id);
            if (message == null)
            {
                return NotFound("Message not found.");
            }
            message.MessageIsRead = true;
            await _sql.SaveChangesAsync();
            
            return Ok("Message marked as read.");
        }

    }
}
