namespace UserManagementService.Tests.Application;

using Moq;
using UserManagementService.Application.UseCases.CommandHandlers.Department;
using UserManagementService.Application.UseCases.CommandHandlers.User;
using UserManagementService.Application.UseCases.Commands.Department;
using UserManagementService.Application.UseCases.Commands.User;
using UserManagementService.Domain.Abstractions.IRepository;
using UserManagementService.Domain.Abstractions.IRabbitMq;
using UserManagementService.Domain.Models;
using Xunit;

public class MembershipCommitTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AddPublishesOnlyAfterSuccessfulCommit(bool failCommit)
    {
        var user = new User();
        var department = new Department();
        var users = new Mock<IUserRepository>();
        var departments = new Mock<IDepartmentRepository>();
        var jobs = new Mock<IUserJobsRepository>();
        var changes = new Mock<IDataChanges>();
        var publisher = new Mock<IUserEventPublisher>();
        users.Setup(repo => repo.GetByIdAsync(user.Id)).ReturnsAsync(user);
        departments.Setup(repo => repo.GetByIdAsync(department.Id)).ReturnsAsync(department);
        var sequence = new MockSequence();
        if (failCommit) changes.Setup(db => db.SaveChangesAsync(default)).ThrowsAsync(new InvalidOperationException("database failure"));
        else
        {
            changes.InSequence(sequence).Setup(db => db.SaveChangesAsync(default)).ReturnsAsync(1);
            publisher.InSequence(sequence).Setup(events => events.PublishUserCreated(user.Id, department.Id)).Returns(Task.CompletedTask);
        }
        var handler = new AddUserToDepartmentCommandHandler(users.Object, departments.Object, jobs.Object, publisher.Object, changes.Object);
        var command = new AddUserToDepartmentCommand(user.Id, department.Id, "doctor", "active", null, null);
        if (failCommit)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, default));
            publisher.Verify(events => events.PublishUserCreated(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
        else
        {
            await handler.Handle(command, default);
            changes.Verify(db => db.SaveChangesAsync(default), Times.Once);
            publisher.Verify(events => events.PublishUserCreated(user.Id, department.Id), Times.Once);
        }
    }

    [Fact]
    public async Task DepartmentDeletionPublishesEverySnapshotPairAfterOneCommit()
    {
        var department = new Department();
        var jobs = new[] { new UserJob { UserId = EntityId.New(), DepartmentId = department.Id }, new UserJob { UserId = EntityId.New(), DepartmentId = department.Id } };
        var departments = new Mock<IDepartmentRepository>();
        var memberships = new Mock<IUserJobsRepository>();
        var changes = new Mock<IDataChanges>();
        var publisher = new Mock<IUserEventPublisher>();
        var committed = false;
        departments.Setup(repo => repo.GetByIdAsync(department.Id)).ReturnsAsync(department);
        memberships.Setup(repo => repo.GetUserJobsByDepartmentIdAsync(department.Id)).ReturnsAsync(jobs);
        changes.Setup(db => db.SaveChangesAsync(default)).Callback(() => committed = true).ReturnsAsync(3);
        publisher.Setup(events => events.PublishUserDeleted(It.IsAny<string>(), department.Id)).Callback(() => Assert.True(committed)).Returns(Task.CompletedTask);
        await new DeleteDepartmentCommandHandler(departments.Object, memberships.Object, publisher.Object, changes.Object)
            .Handle(new DeleteDepartmentCommand(department.Id), default);
        changes.Verify(db => db.SaveChangesAsync(default), Times.Once);
        foreach (var job in jobs) publisher.Verify(events => events.PublishUserDeleted(job.UserId, department.Id), Times.Once);
    }

    [Fact]
    public async Task FailedUserDeletionCommitDoesNotPublish()
    {
        var user = new User();
        var users = new Mock<IUserRepository>();
        var jobs = new Mock<IUserJobsRepository>();
        var publisher = new Mock<IUserEventPublisher>();
        var changes = new Mock<IDataChanges>();
        users.Setup(repo => repo.GetByIdAsync(user.Id)).ReturnsAsync(user);
        jobs.Setup(repo => repo.GetUserJobsByUserIdAsync(user.Id)).ReturnsAsync(new[] { new UserJob { UserId = user.Id, DepartmentId = EntityId.New() } });
        changes.Setup(db => db.SaveChangesAsync(default)).ThrowsAsync(new InvalidOperationException("failure"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => new DeleteUserCommandHandler(users.Object, jobs.Object, publisher.Object, changes.Object)
            .Handle(new DeleteUserCommand(user.Id), default));
        publisher.Verify(events => events.PublishUserDeleted(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
