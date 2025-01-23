namespace ScheduleService.DataAccess.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Metrics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using ScheduleService.DataAccess.Database;
    using ScheduleService.Domain.Abstractions;
    using ScheduleService.Domain.Models;

    public class CalendarRepository : GenericRepository<Calendar>, ICalendarRepository
    {
        public CalendarRepository(DbContext context, DbOptions options)
            : base(context, options.CalendarCollection)
        {
        }
    }
}
