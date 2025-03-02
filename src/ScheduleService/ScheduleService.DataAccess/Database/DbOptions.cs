namespace ScheduleService.DataAccess.Database
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class DbOptions
    {
        public string DatabaseName { get; set; } = string.Empty;

        public string ConnectionString { get; set; } = string.Empty;

        public string CalendarCollection { get; set; } = string.Empty;

        public string UserScheduleRuleCollection { get; set; } = string.Empty;

        public string ScheduleInfoCollection { get; set; } = string.Empty;

        public string ScheduleCollection { get; set; } = string.Empty;
    }
}
