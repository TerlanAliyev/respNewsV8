using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using respNewsV8.Models;

namespace respNewsV8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscribersController : ControllerBase
    {
        private readonly RespNewContext _sql;

        public SubscribersController(RespNewContext sql)
        {
            _sql = sql;
        }

        [HttpGet]
        public List<Subscriber> Get()
        {

            return _sql.Subscribers.ToList();

        }


        [HttpPost]
        public IActionResult Post(Subscriber subscriber)
        {
            subscriber.SubDate = DateTime.Now;
            _sql.Subscribers.Add(subscriber);
            _sql.SaveChanges();
            return Ok();

        }

    }
}

