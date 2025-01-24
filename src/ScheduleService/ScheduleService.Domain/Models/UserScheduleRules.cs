namespace ScheduleService.Domain.Models
{
    using MongoDB.Bson.Serialization.Attributes;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Contains rules for schedule generation and schedule.
    /// </summary>
    public class UserScheduleRules : Entity
    {
        public string UserId { get; set; }

        public string DepartmentId { get; set; }

        public string Month { get; set; } = string.Empty;

        public float HoursPerMonth { get; set; }

        public float MaxHoursPerDay { get; set; }

        /// <summary>
        /// even Day of week.
        /// Tuesday, Thursday.
        /// </summary>
        public bool EvenDOW { get; set; }

        /// <summary>
        /// uneven Day of week.
        /// Monday, Wednesday, Friday.
        /// </summary>
        public bool UnEvenDOW { get; set; }

        /// <summary>
        /// even Day of month.
        /// </summary>
        public bool EvenDOM { get; set; }

        /// <summary>
        /// even Day of month.
        /// </summary>
        public bool UnEvenDOM { get; set; }

        public bool FirstShift { get; set; }

        public string ScheduleId { get; set; }

        // вынести
        public List<WorkDay> Schedule = new List<WorkDay>();
    }
}
