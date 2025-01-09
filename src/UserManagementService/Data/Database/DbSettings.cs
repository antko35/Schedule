namespace UserManagementService.Data.Database;

public class DbSettings : IDbSettings
{
    public string DatabaseName { get; set; } = string.Empty;

    public string ConnectionString { get; set; } = string.Empty;
}
