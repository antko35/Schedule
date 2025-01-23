namespace ScheduleService.Application.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class AddWorkDayDto
    {
        public int Year { get; set; }

        public string MonthName { get; set; }

        public int Month { get; set; }

        public int Day { get; set; }

        public string UserId { get; set; }

        public string DepartmentId { get; set; }
    }
}
