using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using XOProject.Repository.Domain;
using XOProject.Repository.Exchange;
using XOProject.Services.Domain;
using System.Globalization;

namespace XOProject.Services.Exchange
{
    public class AnalyticsService : GenericService<HourlyShareRate>, IAnalyticsService
    {
        private readonly IShareRepository _shareRepository;

        public AnalyticsService(IShareRepository shareRepository) : base(shareRepository)
        {
            _shareRepository = shareRepository;
        }

        public async Task<AnalyticsPrice> GetDailyAsync(string symbol, DateTime day)
        {
            var HourlyShareRateList = await EntityRepository
                 .Query()
                 .Where(x => x.Symbol.Equals(symbol) && x.TimeStamp >= day && x.TimeStamp < day.AddDays(1)).ToListAsync();
            var open = HourlyShareRateList.OrderBy(x => x.TimeStamp).FirstOrDefault();
            var close = HourlyShareRateList.OrderByDescending(x => x.TimeStamp).FirstOrDefault();
            var high = HourlyShareRateList.OrderByDescending(x => x.Rate).FirstOrDefault();
            var low = HourlyShareRateList.OrderBy(x => x.Rate).FirstOrDefault();
            return new AnalyticsPrice()
            {
                Open = open != null ? open.Rate : 0,
                Close = close != null ? close.Rate : 0,
                High = high != null ? high.Rate : 0,
                Low = low != null ? low.Rate : 0
            };
        }

        public async Task<AnalyticsPrice> GetWeeklyAsync(string symbol, int year, int week)
        {
            var firstDateOfWeek = FirstDateOfWeekISO8601(year, week);
            var lastDateOfWeek = firstDateOfWeek.AddDays(6);
            var HourlyShareRateList = await EntityRepository
                .Query()
                .Where(x => x.Symbol.Equals(symbol) && x.TimeStamp >= firstDateOfWeek && x.TimeStamp <= lastDateOfWeek).ToListAsync();
            var open = HourlyShareRateList.OrderBy(x => x.TimeStamp).FirstOrDefault();
            var close = HourlyShareRateList.OrderByDescending(x => x.TimeStamp).FirstOrDefault();
            var high = HourlyShareRateList.OrderByDescending(x => x.Rate).FirstOrDefault();
            var low = HourlyShareRateList.OrderBy(x => x.Rate).FirstOrDefault();
            return new AnalyticsPrice()
            {
                Open = open != null ? open.Rate : 0,
                Close = close != null ? close.Rate : 0,
                High = high != null ? high.Rate : 0,
                Low = low != null ? low.Rate : 0
            };
        }

        public async Task<AnalyticsPrice> GetMonthlyAsync(string symbol, int year, int month)
        {
            var monthDateTime = new DateTime(year, month, 1);
            var HourlyShareRateList = await EntityRepository
                .Query()
                .Where(x => x.Symbol.Equals(symbol) && x.TimeStamp >= monthDateTime && x.TimeStamp < monthDateTime.AddMonths(1)).ToListAsync();
            var open = HourlyShareRateList.OrderBy(x => x.TimeStamp).FirstOrDefault();
            var close = HourlyShareRateList.OrderByDescending(x => x.TimeStamp).FirstOrDefault();
            var high = HourlyShareRateList.OrderByDescending(x => x.Rate).FirstOrDefault();
            var low = HourlyShareRateList.OrderBy(x => x.Rate).FirstOrDefault();
            return new AnalyticsPrice()
            {
                Open = open != null ? open.Rate : 0,
                Close = close != null ? close.Rate : 0,
                High = high != null ? high.Rate : 0,
                Low = low != null ? low.Rate : 0
            };
        }

        public static DateTime FirstDateOfWeekISO8601(int year, int weekOfYear)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            // Use first Thursday in January to get first week of the year as
            // it will never be in Week 52/53
            DateTime firstThursday = jan1.AddDays(daysOffset);
            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekOfYear;
            // As we're adding days to a date in Week 1,
            // we need to subtract 1 in order to get the right date for week #1
            if (firstWeek == 1)
            {
                weekNum -= 1;
            }

            // Using the first Thursday as starting week ensures that we are starting in the right year
            // then we add number of weeks multiplied with days
            var result = firstThursday.AddDays(weekNum * 7);

            // Subtract 3 days from Thursday to get Monday, which is the first weekday in ISO8601
            return result.AddDays(-3);
        }
    }
}