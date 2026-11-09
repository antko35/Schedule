namespace UserManagementService.Tests.Domain;

using UserManagementService.Domain.Models;
using Xunit;

public class EntityIdTests
{
    [Fact]
    public void NewEntitiesHaveIndependentCompatibleIds()
    {
        var entities = Enumerable.Range(0, 1000).Select(_ => new User()).ToList();
        Assert.Equal(entities.Count, entities.Select(entity => entity.Id).Distinct().Count());
        Assert.All(entities, entity => Assert.Matches("^[0-9a-f]{24}$", entity.Id));
        Assert.Matches("^[0-9a-f]{24}$", new Department().Id);
        Assert.Matches("^[0-9a-f]{24}$", new UserJob().Id);
        Assert.Matches("^[0-9a-f]{24}$", new Clinic().Id);
    }
}
