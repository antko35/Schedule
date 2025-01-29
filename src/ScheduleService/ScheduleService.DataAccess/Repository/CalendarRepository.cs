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

        public async Task<List<Calendar>> GetMonthHolidays(int month)
        {
            var filter = Builders<Calendar>.Filter.Eq(x => x.MonthOfHoliday, month);

            var result = await dbSet.Find(filter).ToListAsync();

            return result;
        }

        public async Task<List<Calendar>> GetMonthTransferDays(int month)
        {
            var filter = Builders<Calendar>.Filter.Eq(x => x.MonthOfTransferDay, month);

            var result = await dbSet.Find(filter).ToListAsync();

            return result;
        }
    }
}
