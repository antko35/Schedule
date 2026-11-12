namespace UserManagementService.Application.UseCases.ValidatorsTests.User;

using FluentAssertions;

using UserManagementService.Application.UseCases.Queries.User;
using UserManagementService.Application.UseCases.Validators.Queries.User;
using Xunit;

public class GetAvailableUsersForDepartmentQueryValidatorTests
{
    [Theory]
    [InlineData("")]
    [InlineData("not-an-object-id")]
    public void Validate_ShouldRejectInvalidDepartmentId(string departmentId)
    {
        var query = new GetAvailableUsersForDepartmentQuery(departmentId);

        var result = new GetAvailableUsersForDepartmentQueryValidator().Validate(query);

        result.Errors.Should().Contain(error => error.PropertyName == "DepartmentId");
    }

    [Fact]
    public void Validate_ShouldAcceptValidDepartmentId()
    {
        var query = new GetAvailableUsersForDepartmentQuery(UserManagementService.Domain.Models.EntityId.New());

        var result = new GetAvailableUsersForDepartmentQueryValidator().Validate(query);

        result.IsValid.Should().BeTrue();
    }
}
