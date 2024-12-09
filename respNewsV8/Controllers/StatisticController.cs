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


        //en cox oxunan xeberler
        [HttpGet("TopViewedNews")]
        public async Task<IActionResult> GetTopViewedNews()
        {
            var topNews = await _context.News
                .Include(n => n.NewsCategory)
                    .Include(n => n.NewsOwner)
                    .Include(n => n.NewsLang)
                    .Include(n => n.NewsPhotos)
                    .Include(n => n.NewsVideos)
                    .Include(n => n.NewsTags)
                    .OrderByDescending(n => n.NewsViewCount)
                    .Select(n => new
                    {
                        n.NewsId,
                        n.NewsTitle,
                        n.NewsContetText,
                        n.NewsDate,
                        n.NewsCategory,
                        n.NewsCategoryId,
                        n.NewsViewCount,
                        n.NewsRating,
                        n.NewsOwnerId,
                        n.NewsYoutubeLink,
                        n.NewsTags,
                        n.NewsPhotos,
                        n.NewsVideos,
                        n.NewsLang,
                        n.NewsLangId,
                    })
                .OrderByDescending(n => n.NewsViewCount)
                .Take(10)
                .ToListAsync();
            return Ok(topNews);
        }

        //Gunluk ve ayliq ziyaret
        [HttpGet("VisitStats")]
        public async Task<IActionResult> GetVisitStats()
        {
            try
            {
                // Günlük ziyaret sayısı
                var dailyVisits = await _context.Statisticsses
                    .GroupBy(s => s.VisitDate.Date) 
                    .Select(g => new
                    {
                        Date = g.Key,
                        VisitCount = g.Count()
                    })
                    .ToListAsync();

                // Aylık ziyaret sayısı
                var monthlyVisits = await _context.Statisticsses
                    .GroupBy(s => new { s.VisitDate.Year, s.VisitDate.Month }) 
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        VisitCount = g.Count()
                    })
                    .ToListAsync();

                return Ok(new
                {
                    DailyVisits = dailyVisits,
                    MonthlyVisits = monthlyVisits
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        //Cihaz tipleri
        [HttpGet("DeviceStats")]
        public async Task<IActionResult> GetDeviceStats()
        {
            try
            {
                // Cihaz tipine göre gruplama
                var deviceStats = await _context.Statisticsses
                    .GroupBy(s => new
                    {
                        IsMobile = s.IsMobile ?? false,
                        IsDesktop = s.IsDesktop ?? false
                    })
                    .Select(g => new
                    {
                        DeviceType = g.Key.IsMobile ? "Mobile" : g.Key.IsDesktop ? "Desktop" : "Other",
                        VisitCount = g.Count()
                    })
                    .ToListAsync();

                return Ok(deviceStats);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }


        //Aktiv muxbirler
        [HttpGet("TopOwners")]
        public async Task<IActionResult> GetTopOwners()
        {
            try
            {
                var topOwners = await _context.News
                    .GroupBy(n => n.NewsOwner.OwnerName)
                    .Select(g => new
                    {
                        OwnerName = g.Key,
                        NewsCount = g.Count()
                    })
                    .OrderByDescending(o => o.NewsCount)
                    .Take(10)
                    .ToListAsync();

                return Ok(topOwners);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }


        //En cox baxilan xeberler timePeriod -https://localhost:44314/api/Statistic/GetTopViewedNewsByTimePeriod?timePeriod=1month
        [HttpGet("GetTopViewedNewsByTimePeriod")]
        public async Task<IActionResult> GetTopViewedNewsByTimePeriod([FromQuery] string timePeriod)
        {
            try
            {
                if (string.IsNullOrEmpty(timePeriod))
                {
                    return BadRequest(new { error = "timePeriod parametresi gereklidir." });
                }

                DateTime startDate;

                if (timePeriod == "24hours")
                {
                    startDate = DateTime.Now.AddHours(-24);
                }
                else if (timePeriod == "1month")
                {
                    startDate = DateTime.Now.AddMonths(-1);
                }
                else if (timePeriod == "1year")
                {
                    startDate = DateTime.Now.AddYears(-1);
                }
                else
                {
                    return BadRequest(new { error = "Geçersiz zaman dilimi." });
                }

                var topViewedNews = await _context.News
                    .Include(n => n.NewsCategory)
                    .Include(n => n.NewsOwner)
                    .Include(n => n.NewsLang)
                    .Include(n => n.NewsPhotos)
                    .Include(n => n.NewsVideos)
                    .Include(n => n.NewsTags)
                    .Where(n => n.NewsDate >= startDate)
                    .OrderByDescending(n => n.NewsViewCount)
                    .OrderByDescending(n=>n.NewsDate)
                    .OrderByDescending(n => n.NewsRating)
                    .Select(n => new
                    {
                        n.NewsId,
                        n.NewsTitle,
                        n.NewsContetText,
                        n.NewsDate,
                        n.NewsCategory,
                        n.NewsCategoryId,
                        n.NewsViewCount,
                        n.NewsRating,
                        n.NewsOwnerId,
                        n.NewsYoutubeLink,
                        n.NewsTags,
                        n.NewsPhotos,
                        n.NewsVideos,
                        n.NewsLang,
                        n.NewsLangId,
                    })
                    .Take(10)
                    .ToListAsync();

                return Ok(topViewedNews);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }







    }
}
