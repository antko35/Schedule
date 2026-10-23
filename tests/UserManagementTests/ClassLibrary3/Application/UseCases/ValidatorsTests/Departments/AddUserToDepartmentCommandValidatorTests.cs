namespace UserManagementService.Application.UseCases.ValidatorsTests.Departments;

using FluentAssertions;
using MongoDB.Bson;
using UserManagementService.Application.UseCases.Commands.Department;
using UserManagementService.Application.UseCases.Validators.Commands.Departments;
using Xunit;

public class AddUserToDepartmentCommandValidatorTests
{
    public static TheoryData<AddUserToDepartmentCommand, string> RequiredFieldCases =>
        new TheoryData<AddUserToDepartmentCommand, string>()
    {
        { CreateValidCommand() with { UserId = string.Empty }, "UserId" },
        { CreateValidCommand() with { DepartmentId = string.Empty }, "DepartmentId" },
        { CreateValidCommand() with { Role = string.Empty }, "Role" },
        { CreateValidCommand() with { Status = string.Empty }, "Status" },
    };

    [Theory]
    [MemberData(nameof(RequiredFieldCases))]
    public void Validate_ShouldRejectMissingRequiredField(
        AddUserToDepartmentCommand command,
        string propertyName)
    {
        var result = new AddUserToDepartmentCommandValidator().Validate(command);

        result.Errors.Should().Contain(error => error.PropertyName == propertyName);
    }

    [Fact]
    public void Validate_ShouldRejectMalformedIdsEmailAndPhoneNumber()
    {
        var command = CreateValidCommand() with
        {
            UserId = "not-an-object-id",
            DepartmentId = "also-invalid",
            Email = "invalid-email",
            PhoneNumber = "+375291234567",
        };

        var result = new AddUserToDepartmentCommandValidator().Validate(command);

        result.Errors.Select(error => error.PropertyName).Should().Contain(
            ["UserId", "DepartmentId", "Email", "PhoneNumber"]);
    }

    [Theory]
    [InlineData("+375291234567")]
    [InlineData("+375-2-1234567")]
    [InlineData("+375-29-123456")]
    [InlineData("+375-29-12345678")]
    [InlineData("375-29-1234567")]
    [InlineData("+375-٢٩-١٢٣٤٥٦٧")]
    [InlineData("+375-29-1234567\n")]
    public void Validate_ShouldRejectPhoneOutsideDocumentedFormat(string phoneNumber)
    {
        var command = CreateValidCommand() with { PhoneNumber = phoneNumber };

        var result = new AddUserToDepartmentCommandValidator().Validate(command);

        result.Errors.Should().Contain(error => error.PropertyName == "PhoneNumber");
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("   ", "   ")]
    public void Validate_ShouldAcceptMissingOptionalContactDetails(string? email, string? phoneNumber)
    {
        var command = CreateValidCommand() with { Email = email, PhoneNumber = phoneNumber };

        var result = new AddUserToDepartmentCommandValidator().Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldNotRestrictNonEmptyRoleAndStatusValues()
    {
        var command = CreateValidCommand() with
        {
            Role = "custom-role",
            Status = "custom-status",
        };

        var result = new AddUserToDepartmentCommandValidator().Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldAcceptCompleteCommand()
    {
        var result = new AddUserToDepartmentCommandValidator().Validate(CreateValidCommand());

        result.IsValid.Should().BeTrue();
    }

    private static AddUserToDepartmentCommand CreateValidCommand()
    {
        return new AddUserToDepartmentCommand(
            ObjectId.GenerateNewId().ToString(),
            ObjectId.GenerateNewId().ToString(),
            "doctor",
            "Работа",
            "doctor@example.com",
            "+375-29-1234567");
    }
}
