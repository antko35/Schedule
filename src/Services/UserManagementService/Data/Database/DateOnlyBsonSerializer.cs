namespace UserManagementService.DataAccess.Database;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

public class DateOnlyBsonSerializer : StructSerializerBase<DateOnly>
{
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateOnly value)
        => context.Writer.WriteString(value.ToString("yyyy-MM-dd"));

    public override DateOnly Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        => DateOnly.Parse(context.Reader.ReadString());
}
