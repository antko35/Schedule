namespace UserManagementService.DataAccess.Database;

public class DbOptions : IDbOptions
{
    public string DatabaseName { get; set; } = string.Empty;

    public string ConnectionString { get; set; } = string.Empty;

    public string UsersCollectionName { get; set; } = string.Empty;

    public string DepartmentsCollectionName { get; set; } = string.Empty;

    public string UserJobsCollectionName { get; set; } = string.Empty;
}
