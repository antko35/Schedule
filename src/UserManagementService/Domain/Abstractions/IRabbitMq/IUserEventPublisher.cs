namespace UserManagementService.Domain.Abstractions.IRabbitMq;

public interface IUserEventPublisher
{
    Task PublishUserCreated(string userId, string departmentId);

    Task PublishUserDeleted(string userId, string departmentId);
}