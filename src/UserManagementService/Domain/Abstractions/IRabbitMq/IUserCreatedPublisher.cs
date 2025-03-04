namespace UserManagementService.Domain.Abstractions.IRabbitMq;

public interface IUserCreatedPublisher
{
    Task PublishUserCreated(string userId, string departmentId);
}