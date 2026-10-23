namespace ScheduleService.Domain.Models;

using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

public class Schedule : Entity
{
    [BsonElement("monthName")]
    public string MonthName { get; set; } = string.Empty;

    [BsonElement("workDays")]
    public List<WorkDay> WorkDays { get; set; } = new List<WorkDay>();
}