namespace ScheduleService.Domain.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    using MongoDB.Driver;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Contains rules for schedule generation.
    /// </summary>
    public class UserScheduleRules : Entity
    {
        [BsonElement("userId")]
        public string UserId { get; set; }

        [BsonElement("departmentId")]
        public string DepartmentId { get; set; }

        [BsonElement("monthName")]
        public string MonthName { get; set; } = string.Empty;

        [BsonElement("year")]
        public int Year { get; set; }

        [BsonElement("hoursPerMonth")]
        public float HoursPerMonth { get; set; }

        [BsonElement("hoursPerDay")]
        public float MaxHoursPerDay { get; set; }

        /// <summary>
        /// Work first shift on even Day of week and second shift on uneven days.
        /// Tuesday, Thursday.
        /// </summary>
        public bool EvenDOW { get; set; }

        /// <summary>
        /// uneven Day of week.
        /// Monday, Wednesday, Friday.
        /// </summary>
        public bool UnEvenDOW { get; set; }

        /// <summary>
        /// Work first shift on even Day of month and second shift on uneven days.
        /// even Day of month.
        /// </summary>
        public bool EvenDOM { get; set; }

        /// <summary>
        /// Uneven Day of month.
        /// </summary>
        public bool UnEvenDOM { get; set; }

        public bool FirstShift { get; set; }

        public bool SecondShift { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ScheduleId { get; set; }
    }
}
