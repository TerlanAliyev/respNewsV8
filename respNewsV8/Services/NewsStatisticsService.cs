using respNewsV8.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace respNewsV8.Services
{
    public class NewsStatisticsService
    {
        private readonly RespNewContext _sql;

        public NewsStatisticsService(RespNewContext sql)
        {
            _sql = sql;
        }

        public async Task<List<OwnerYearlyMonthlyStatsDto>> GetOwnerYearlyMonthlyStatisticsAsync()
        {
            var statistics = await (from n in _sql.News
                                    join o in _sql.Owners on n.NewsOwnerId equals o.OwnerId
                                    where n.NewsDate.HasValue
                                    group n by new { o.OwnerName, Year = n.NewsDate.Value.Year, Month = n.NewsDate.Value.Month } into g
                                    select new
                                    {
                                        OwnerName = g.Key.OwnerName,
                                        Year = g.Key.Year,
                                        Month = g.Key.Month,
                                        NewsCount = g.Count()
                                    })
                                     .ToListAsync();

            var result = statistics
                .GroupBy(s => s.OwnerName)
                .Select(g => new OwnerYearlyMonthlyStatsDto
                {
                    OwnerName = g.Key,
                    YearlyStats = g.GroupBy(x => x.Year)
                        .ToDictionary(
                            yearGroup => yearGroup.Key,
                            yearGroup => new MonthlyStats
                            {
                                January = yearGroup.Where(x => x.Month == 1).Sum(x => x.NewsCount),
                                February = yearGroup.Where(x => x.Month == 2).Sum(x => x.NewsCount),
                                March = yearGroup.Where(x => x.Month == 3).Sum(x => x.NewsCount),
                                April = yearGroup.Where(x => x.Month == 4).Sum(x => x.NewsCount),
                                May = yearGroup.Where(x => x.Month == 5).Sum(x => x.NewsCount),
                                June = yearGroup.Where(x => x.Month == 6).Sum(x => x.NewsCount),
                                July = yearGroup.Where(x => x.Month == 7).Sum(x => x.NewsCount),
                                August = yearGroup.Where(x => x.Month == 8).Sum(x => x.NewsCount),
                                September = yearGroup.Where(x => x.Month == 9).Sum(x => x.NewsCount),
                                October = yearGroup.Where(x => x.Month == 10).Sum(x => x.NewsCount),
                                November = yearGroup.Where(x => x.Month == 11).Sum(x => x.NewsCount),
                                December = yearGroup.Where(x => x.Month == 12).Sum(x => x.NewsCount)
                            })
                })
                .ToList();

            return result;
        }
        public async Task<List<CategoryNewsCountDto>> GetCategoryNewsCountAsync()
        {
            var result = await _sql.News
                .GroupBy(n => n.NewsCategory.CategoryName) 
                .Select(group => new CategoryNewsCountDto
                {
                    CategoryName = group.Key,  
                    NewsCount = group.Count()  
                })
                .ToListAsync();

            return result;
        }



    }
}
