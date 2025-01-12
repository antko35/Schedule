using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace UserManagementService.Domain.Models;

public class UserJob : Entity
{
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("userId")]
    public string? UserId { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("departmentId")]
    public string? DepartmentId { get; set; }

    [BsonElement("role")]
    public string? Role { get; set; }

    [BsonElement("status")]
    public string? Status { get; set; }
}
