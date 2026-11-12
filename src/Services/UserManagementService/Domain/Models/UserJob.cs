
namespace UserManagementService.Domain.Models;

public class UserJob : Entity
{


    public string UserId { get; set; } = string.Empty;


    public string? DepartmentId { get; set; }

    public string? Role { get; set; }

    public string? Status { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }
}
