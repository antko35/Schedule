using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace UserManagementService.Domain.Models
{
    public class Entity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
    }
}