namespace UserManagementService.Domain.Abstractions.IRepository;

public interface IDataChanges
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
