﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UserManagementService.Domain.Models;
public class Department : Entity
{
    [BsonElement("departmentName")]
    public string? DepartmentName { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("clinicId")]
    public string? ClinicId { get; set; }
}
