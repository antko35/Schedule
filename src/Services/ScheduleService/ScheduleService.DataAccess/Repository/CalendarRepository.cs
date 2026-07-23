namespace ScheduleService.DataAccess.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Metrics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MongoDB.Driver;
    using ScheduleService.DataAccess.Database;
    using ScheduleService.Domain.Abstractions;
    using ScheduleService.Domain.Models;

    public class CalendarRepository : GenericRepository<Calendar>, ICalendarRepository
    {
        public CalendarRepository(DbContext context, DbOptions options)
            : base(context, options.CalendarCollection)
        {
        }

        public async Task<List<Calendar>> GetMonthHolidays(int year, int month)
        {
            var filter = Builders<Calendar>.Filter
                .And(
                Builders<Calendar>.Filter.Eq(x => x.MonthOfHoliday, month),
                Builders<Calendar>.Filter.Eq(x => x.Year, year));

            var result = await dbSet.FindAsync(filter);

            return await result.ToListAsync();
        }

        public async Task<List<Calendar>> GetMonthTransferDays(int year, int month)
        {
            var filter = Builders<Calendar>.Filter
                .And(
                Builders<Calendar>.Filter.Eq(x => x.Year, year),
                Builders<Calendar>.Filter.Eq(x => x.MonthOfTransferDay, month),
                Builders<Calendar>.Filter.Ne(x => x.MonthOfTransferDay, 0));

            var result = await dbSet.Find(filter).ToListAsync();

            return result;
        }

        public async Task<List<Calendar>> GetYearHolidays(int year)
        {
            var filter = Builders<Calendar>.Filter.Eq(x => x.Year, year);

            var result = await dbSet.Find(filter).ToListAsync();

            return result;
        }
    }
}
