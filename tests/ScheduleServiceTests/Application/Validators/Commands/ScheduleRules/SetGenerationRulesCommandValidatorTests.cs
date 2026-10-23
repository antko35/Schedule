using FluentAssertions;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;
using ScheduleService.Application.Validators.Commands.ScheduleRules;
using Xunit;

namespace Application.Validators.Commands.ScheduleRules;

public class SetGenerationRulesCommandValidatorTests
{
    private readonly SetGenerationRulesCommandValidator validator = new SetGenerationRulesCommandValidator();

    [Fact]
    public void Validate_ShouldSucceed_WhenOptionalValuesAreOmitted()
    {
        var command = new SetGenerationRulesCommand
        {
            ScheduleRulesId = "rule123",
        };

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldSucceed_WhenAllPeriodicityFlagsAreFalse()
    {
        var command = new SetGenerationRulesCommand
        {
            ScheduleRulesId = "rule123",
            OnlyFirstShift = false,
            OnlySecondShift = false,
            EvenDOW = false,
            UnEvenDOW = false,
            EvenDOM = false,
            UnEvenDOM = false,
        };

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldSucceed_WhenExactlyOnePeriodicityFlagIsTrue()
    {
        var command = new SetGenerationRulesCommand
        {
            ScheduleRulesId = "rule123",
            EvenDOW = true,
        };

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenMultiplePeriodicityFlagsAreTrue()
    {
        var command = new SetGenerationRulesCommand
        {
            ScheduleRulesId = "rule123",
            EvenDOW = true,
            OnlyFirstShift = true,
        };

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(error => error.ErrorMessage.StartsWith("Only one of"));
    }
}
