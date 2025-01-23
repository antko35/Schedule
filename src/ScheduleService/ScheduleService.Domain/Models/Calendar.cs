namespace ScheduleService.Domain.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MongoDB.Bson.Serialization.Attributes;

    public class Calendar : Entity
    {
        [BsonElement("dayOfMonth")]
        public int DayOfMonth { get; set; }

        [BsonElement("dayOfWeek")]
        public DayOfWeek DayOfWeek { get; set; }

        [BsonElement("officialHoliday")]
        public bool OfficialHoliday { get; set; }

        [BsonElement("transferDay")]
        public DateOnly? TransferDay { get; set; } // перенос на какую дату
    }
}
