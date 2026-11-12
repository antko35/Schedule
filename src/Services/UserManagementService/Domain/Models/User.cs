
namespace UserManagementService.Domain.Models;

public class User : Entity
{

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Patronymic { get; set; }

    public string? Gender { get; set; }

    public int Age { get; private set; }

    private DateOnly dateOfBirth;

    public DateOnly DateOfBirth
    {
        get => dateOfBirth;
        set
        {
            dateOfBirth = value;
            CalculateAge();
        }
    }

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