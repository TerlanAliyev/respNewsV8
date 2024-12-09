using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using respNewsV8.Models;

namespace respNewsV8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewspaperController : ControllerBase
    {
        private readonly RespNewContext _sql;

        public NewspaperController(RespNewContext sql)
        {
            _sql = sql;
        }

        // GET BY ID
        [HttpGet("{id}")]
        public IActionResult GetNewspaperById(int id)
        {
            var newspaper = _sql.Newspapers.Find(id);
            if (newspaper == null)
                return NotFound("Gazete bulunamadı.");

            return Ok(newspaper);
        }

        // GET ALL
        [HttpGet]
        public IActionResult GetAllNewspapers()
        {
            var newspapers = _sql.Newspapers.OrderByDescending(x=>x.NewspaperDate).ToList();
            return Ok(newspapers);
        }




        //umumi
        [HttpGet("count")]
        public IActionResult GetNewsPaperCount()
        {
            // Kategorilerin sayısını almak
            var NewsPaperCount = _sql.Newspapers
                .Select(x => x.NewspaperId)  // Kategori ismi
                .Distinct()                   // Benzersiz kategoriler
                .Count();                     // Sayma işlemi

            return Ok(new { NewsPaperCount });  // JSON formatında sayıyı döndürme


        }




        // GET LAST
        [HttpGet("last")]
        public IActionResult GetLastNewspapers()
        {
            var Lastnewspapers = _sql.Newspapers.OrderByDescending(x=>x.NewspaperDate).FirstOrDefault();
            return Ok(Lastnewspapers);
        }


        // GET PDF URL
        [HttpGet("pdf/{id}")]
        public IActionResult GetPdf(int id)
        {
            var newspaper = _sql.Newspapers.Find(id);
            if (newspaper == null)
                return NotFound("Gazete bulunamadı.");

            if (string.IsNullOrEmpty(newspaper.NewspaperPdfUrl))
                return NotFound("PDF URL'si bulunamadı.");

            return Redirect(newspaper.NewspaperPdfUrl);
        }



        // POST
        [HttpPost]
        public async Task<IActionResult> CreateNewspaper([FromForm] newspaperDto newspaperDto)
        {
            if (newspaperDto == null)
                return BadRequest("Geçersiz gazete verisi.");

            var newspaper = new Newspaper
            {
                NewspaperTitle = newspaperDto.NewspaperTitle,
                NewspaperLinkFlip = newspaperDto.NewspaperLinkFlip,
                NewspaperStatus = true,
                NewspaperPrice = newspaperDto.NewspaperPrice,
                NewspaperDate = newspaperDto.NewspaperDate
            };

            // Cover upload
            if (newspaperDto.NewspaperCoverUrl != null && newspaperDto.NewspaperCoverUrl.Length > 0)
            {
                var coverFileExtension = Path.GetExtension(newspaperDto.NewspaperCoverUrl.FileName);
                var coverFileName = Guid.NewGuid().ToString() + coverFileExtension; // Rastgele isim oluştur
                var coverUploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "NewspaperCovers");

                if (!Directory.Exists(coverUploadsFolder))
                {
                    Directory.CreateDirectory(coverUploadsFolder);
                }

                var coverFilePath = Path.Combine(coverUploadsFolder, coverFileName);

                using (var stream = new FileStream(coverFilePath, FileMode.Create))
                {
                    await newspaperDto.NewspaperCoverUrl.CopyToAsync(stream);
                }

                newspaper.NewspaperCoverUrl = $"/NewspaperCovers/{coverFileName}";
            }
            else
            {
                return BadRequest("Kapak dosyası yüklenmedi.");
            }

            // PDF document upload
            if (newspaperDto.NewspaperPdfFile != null && newspaperDto.NewspaperPdfFile.Length > 0)
            {
                var pdfFileExtension = Path.GetExtension(newspaperDto.NewspaperPdfFile.FileName);
                var pdfFileName = Guid.NewGuid().ToString() + pdfFileExtension; // Rastgele isim oluştur
                var pdfUploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "NewspaperPDF");

                if (!Directory.Exists(pdfUploadsFolder))
                {
                    Directory.CreateDirectory(pdfUploadsFolder);
                }

                var pdfFilePath = Path.Combine(pdfUploadsFolder, pdfFileName);

                using (var stream = new FileStream(pdfFilePath, FileMode.Create))
                {
                    await newspaperDto.NewspaperPdfFile.CopyToAsync(stream);
                }

                newspaper.NewspaperPdfUrl = $"/NewspaperPDF/{pdfFileName}";
            }
            else
            {
                return BadRequest("PDF dosyası yüklenmedi.");
            }

            try
            {
                _sql.Newspapers.Add(newspaper);
                await _sql.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Sunucu hatası: {ex.Message}");
            }

            return CreatedAtAction(nameof(GetNewspaperById), new { id = newspaper.NewspaperId }, newspaper);
        }


    }
}

