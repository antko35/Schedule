namespace UserManagementService.Data.Database;

public interface IDbSettings
{
    public string DatabaseName { get; set; }

    public string ConnectionString { get; set; }
}
