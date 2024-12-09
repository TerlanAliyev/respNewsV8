using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using respNewsV8.Models;
using respNewsV8.Helper;
using System.Linq;
using System.Threading.Tasks;
using respNewsV8.Services;

namespace respNewsV8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticController : ControllerBase
    {
        private readonly RespNewContext _context;
        private readonly GeoLocationService _geoLocationService;
        private readonly NewsStatisticsService _newsStatisticsService;


        public StatisticController(RespNewContext context, GeoLocationService geoLocationService, NewsStatisticsService newsStatisticsService)
        {
            _context = context;
            _geoLocationService = geoLocationService;
            _newsStatisticsService = newsStatisticsService;
        }

        //  (POST)
        [HttpPost("AddStatistic")]
        public async Task<IActionResult> AddStatistic([FromBody] Statisticss statistic)
        {
            if (statistic == null || string.IsNullOrEmpty(statistic.VisitorIp))
            {
                return BadRequest("Geçersiz veri.");
            }

            try
            {
                _context.Statisticsses.Add(statistic);
                await _context.SaveChangesAsync();
                return Ok(new { message = "İstatistik başarıyla eklendi." });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        //ALL (GET)
        [HttpGet("GetStatistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var statistics = await _context.Statisticsses.ToListAsync();
            return Ok(statistics);
        }

        // IP sorgulama (GET)
        [HttpGet("GetStatisticByIP/{ip}")]
        public async Task<IActionResult> GetStatisticByIP(string ip)
        {
            var statistics = await _context.Statisticsses
                .Where(s => s.VisitorIp == ip)
                .ToListAsync();

            if (!statistics.Any())
            {
                return NotFound("Bu IP adresine ait istatistik bulunamadı.");
            }

            return Ok(statistics);
        }

        // Tarixe gore sorgulama (GET)
        [HttpGet("GetStatisticsByDate/{date}")]
        public async Task<IActionResult> GetStatisticsByDate(string date)
        {
            var statistics = await _context.Statisticsses
                .Where(s => s.VisitDate.ToString("yyyy-MM-dd") == date)
                .ToListAsync();

            if (!statistics.Any())
            {
                return NotFound("Bu tarihe ait istatistik bulunamadı.");
            }

            return Ok(statistics);
        }

        [HttpPost("TrackVisit")]
        public async Task<IActionResult> TrackVisit()
        {
            try
            {
                string visitorIP = HttpContext.Connection.RemoteIpAddress?.ToString();
                string userAgent = HttpContext.Request.Headers["User-Agent"];
                string userLanguage = HttpContext.Request.Headers["Accept-Language"].ToString();

                var deviceDetector = new DeviceDetectorNET.DeviceDetector(userAgent);
                deviceDetector.Parse();

                bool isMobile = deviceDetector.IsMobile();
                bool isDesktop = deviceDetector.IsDesktop();
                bool isEnglish = userLanguage.Contains("en");
                bool isAzerbaijani = userLanguage.Contains("az");
                bool isRussian = userLanguage.Contains("ru");

                var locationData = await _geoLocationService.GetLocationFromIP(visitorIP);

                if (locationData == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new { error = "GeoLocation verisi alınamadı" });
                }

                var statistic = new Statisticss
                {
                    VisitorIp = visitorIP,
                    VisitorCountry = locationData?.CountryName, 
                    VisitorCity = locationData?.City,
                    VisitDate = DateTime.Now,
                    IsMobile = isMobile,
                    IsDesktop = isDesktop,
                    IsEngLanguage = isEnglish,
                    IsAzLanguage = isAzerbaijani,
                    IsRuLanguage = isRussian
                };

                _context.Statisticsses.Add(statistic);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Visit tracked successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "Bir hata oluştu.", details = ex.Message });
            }
        }

        // Aylıq Owners Statistikasi
        [HttpGet("OwnerMonthlyStatistics")]
        public async Task<IActionResult> GetOwnerMonthlyStatistics()
        {
            var statistics = await _newsStatisticsService.GetOwnerYearlyMonthlyStatisticsAsync();
            return Ok(statistics);
        }
        //Category Count
        [HttpGet("CategoryNewsCount")]
        public async Task<IActionResult> GetCategoryNewsCount()
        {
            var categoryNewsCount = await _newsStatisticsService.GetCategoryNewsCountAsync();
            return Ok(categoryNewsCount);
        }

        







    }
}
