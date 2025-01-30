namespace ScheduleService.Domain.Models
{
    using MongoDB.Bson.Serialization.Attributes;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class Schedule : Entity
    {
        [BsonElement("monthName")]
        public string MonthName { get; set; }

        [BsonElement("workDays")]
        public List<WorkDay> WorkDays = new List<WorkDay>();
    }
}
