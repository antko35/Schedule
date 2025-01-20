using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UserManagementService.Domain.Models;

public class User : Entity
{
    [BsonElement("firstName")]
    public string? FirstName { get; set; }

    [BsonElement("lastName")]
    public string? LastName { get; set; }

    [BsonElement("patronymic")]
    public string? Patronymic { get; set; }

    [BsonElement("gender")]
    public string? Gender { get; set; }

    [BsonElement("age")]
    public int Age { get; set; }

    [BsonElement("dateOfBirth")]
    public DateOnly DateOfBirth { get; set; }

    /// <summary>
    /// Set updated age
    /// </summary>
    /// <returns>new age</returns>
    public int CalculateAge()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var calculatedAge = today.Year - DateOfBirth.Year;
        if (today < DateOfBirth.AddYears(calculatedAge))
        {
            calculatedAge--;
        }

        Age = calculatedAge;
        return Age;
    }
}