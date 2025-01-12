namespace UserManagementService.DataAccess.Database;

public interface IDbOptions
{
    public string DatabaseName { get; set; }

    public string ConnectionString { get; set; }

    public string UsersCollectionName { get; set; }

    public string DepartmentsCollectionName { get; set; }

    public string UserJobsCollectionName { get; set; }
}
