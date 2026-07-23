namespace UserManagementService.Domain.Models;

public class Clinic : Entity
{
    public string ClinicName { get; set; } = string.Empty;

    public string ClinicDescription { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;
}
