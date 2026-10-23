namespace ScheduleService.Domain.Models
{
    using System;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

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

        [BsonElement("startWorkDayTime")]
        public TimeOnly StartWorkDayTime { get; set; } = new TimeOnly(8, 0, 0);

        [BsonElement("onlyFirstShift")]
        public bool OnlyFirstShift { get; set; }

        [BsonElement("onlySecondShift")]
        public bool OnlySecondShift { get; set; }

        /// <summary>
        /// Work the second shift on Monday, Wednesday, and Friday,
        /// and the first shift on Tuesday and Thursday.
        /// </summary>
        public bool EvenDOW { get; set; }

        /// <summary>
        /// Work the first shift on Monday, Wednesday, and Friday,
        /// and the second shift on Tuesday and Thursday.
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

        [BsonElement("scheduleId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ScheduleId { get; set; }
    }
}
