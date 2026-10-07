namespace UserManagementService.Application.UseCases.ValidatorsTests.Departments;

using FluentAssertions;
using MongoDB.Bson;
using UserManagementService.Application.UseCases.Commands.Department;
using UserManagementService.Application.UseCases.Validators.Commands.Departments;
using Xunit;

public class EditUserInDepartmentCommandValidatorTests
{
    public static TheoryData<EditUserInDepartmentCommand, string> RequiredFieldCases =>
        new TheoryData<EditUserInDepartmentCommand, string>()
        {
            { CreateValidCommand() with { DepartmentId = string.Empty }, "DepartmentId" },
            { CreateValidCommand() with { UserId = string.Empty }, "UserId" },
            { CreateValidCommand() with { Role = string.Empty }, "Role" },
            { CreateValidCommand() with { Status = string.Empty }, "Status" },
        };

    [Theory]
    [MemberData(nameof(RequiredFieldCases))]
    public void Validate_ShouldRejectMissingRequiredField(
        EditUserInDepartmentCommand command,
        string propertyName)
    {
        var result = new EditUserInDepartmentCommandValidator().Validate(command);

        result.Errors.Should().Contain(error => error.PropertyName == propertyName);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("   ", "   ")]
    public void Validate_ShouldAcceptMissingOptionalContactDetails(string? email, string? phoneNumber)
    {
        var command = CreateValidCommand() with { Email = email, PhoneNumber = phoneNumber };

        var result = new EditUserInDepartmentCommandValidator().Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldRejectMalformedContactDetails()
    {
        var command = CreateValidCommand() with
        {
            Email = "invalid-email",
            PhoneNumber = "+375291234567",
        };

        var result = new EditUserInDepartmentCommandValidator().Validate(command);

        result.Errors.Select(error => error.PropertyName).Should().Contain(["Email", "PhoneNumber"]);
    }

    private static EditUserInDepartmentCommand CreateValidCommand()
    {
        return new EditUserInDepartmentCommand(
            ObjectId.GenerateNewId().ToString(),
            ObjectId.GenerateNewId().ToString(),
            "doctor",
            "Работа",
            "doctor@example.com",
            "+375-29-1234567");
    }
}
