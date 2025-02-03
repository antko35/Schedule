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
        [BsonElement("year")]
        public int Year { get; set; }

        [BsonElement("holiday")]
        public DateOnly HolidayDate { get; set; }

        [BsonElement("transferDay")]
        public DateOnly? TransferDate { get; set; }

        [BsonElement("transferDayMonthNumber")]
        public int MonthOfTransferDay { get; set; }

        [BsonElement("holidayMonthNumber")]
        public int MonthOfHoliday { get; set; }

        [BsonElement("dayOfMonth")]
        public int HolidayDayOfMonth { get; set; } = 0;

        [BsonElement("dayOfWeek")]
        public DayOfWeek DayOfWeek { get; set; }
    }
}
