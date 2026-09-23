namespace ScheduleService.Application.UseCases.Commands.Schedule
{
    using MediatR;

    public record GenerateDepartmentScheduleCommand(
        string DepartmentId,
        int Year,
        int Month)
        : IRequest;
}
