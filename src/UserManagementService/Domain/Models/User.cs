using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UserManagementService.Domain.Models;

public class User
{
    [BsonId]
    [BsonElement("_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("first_name")]
    public string? FirstName { get; set; }

    [BsonElement("last_name")]
    public string LastName { get; set; }

    [BsonElement("patronymic")]
    public string Patronymic { get; set; }

    [BsonElement("gender")]
    public string Gender { get; set; }

    [BsonElement("age")]
    public int Age { get; set; }

    // phone, email in auth service
}
