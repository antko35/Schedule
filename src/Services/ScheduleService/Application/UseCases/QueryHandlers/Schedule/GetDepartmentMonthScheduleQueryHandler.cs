namespace ScheduleService.Application.UseCases.QueryHandlers.Schedule;

using MediatR;
using ScheduleService.Application.Contracts;
using ScheduleService.Application.Mapping;
using ScheduleService.Application.UseCases.Queries.Schedule;
using ScheduleService.Domain.Abstractions;

public sealed class GetDepartmentMonthScheduleQueryHandler
    : IRequestHandler<GetDepartmentMonthScheduleQuery, List<ScheduleDto>>
{
    private readonly IScheduleRepository scheduleRepository;

    public GetDepartmentMonthScheduleQueryHandler(IScheduleRepository scheduleRepository)
    {
        this.scheduleRepository = scheduleRepository;
    }

    public async Task<List<ScheduleDto>> Handle(
        GetDepartmentMonthScheduleQuery request,
        CancellationToken cancellationToken)
    {
        var period = new DateOnly(request.Year, request.Month, 1);
        var schedules = await scheduleRepository.GetDepartmentAsync(
            request.DepartmentId,
            period,
            cancellationToken);

        if (schedules.Count == 0)
        {
            throw new InvalidOperationException("Schedule rules not found");
        }

        return schedules.Select(schedule => schedule.ToDto()).ToList();
    }
}
