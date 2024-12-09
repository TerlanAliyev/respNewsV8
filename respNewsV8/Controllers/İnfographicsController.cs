using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Infrastructure;
using respNewsV8.Models;

namespace respNewsV8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class İnfographicsController : ControllerBase
    {
        private readonly RespNewContext _sql;

        public İnfographicsController(RespNewContext sql)
        {
            _sql = sql;
        }


        [HttpGet]
        public IActionResult Get()
        {
            var Infs = _sql.İnfographics.ToList();
            return Ok(Infs);    
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var Infs = _sql.İnfographics.Where(x=>x.InfId==id).ToList();
            _sql.Remove(Infs);
            _sql.SaveChanges();
            return Ok(Infs);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadInfographic([FromForm] InfographicDTO infographicDTO)
        {
            if (infographicDTO.InfPhoto == null || infographicDTO.InfPhoto.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var fileExtension = Path.GetExtension(infographicDTO.InfPhoto.FileName);
            var fileName = Guid.NewGuid().ToString() + fileExtension; // Rastgele isim oluştur
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "InfPhotos", fileName);

            try
            {
                // Dosyayı kaydetme
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await infographicDTO.InfPhoto.CopyToAsync(stream);
                }

                // Veritabanına kaydetme
                var infographic = new İnfographic
                {
                    InfName = infographicDTO.InfName,
                    InfPhoto = $"/InfPhotos/{fileName}", // URL'yi kaydet
                    InfPostDate = DateTime.Now
                };

                _sql.İnfographics.Add(infographic);  // Infographic nesnesini ekle
                await _sql.SaveChangesAsync();  // Değişiklikleri kaydet

                return Ok(new { message = "File uploaded and saved to database", fileUrl = infographic.InfPhoto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal Server Error", error = ex.Message });
            }
        }
















    }
}
