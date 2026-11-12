namespace ScheduleService.Application.UseCases.QueryHandlers.Schedule;

using MediatR;
using ScheduleService.Application.Contracts;
using ScheduleService.Application.Extensions;
using ScheduleService.Application.Mapping;
using ScheduleService.Application.UseCases.Queries.Schedule;
using ScheduleService.Domain.Abstractions;

public sealed class GetUserMonthScheduleQueryHandler
    : IRequestHandler<GetUserMonthScheduleQuery, ScheduleDto>
{
    private readonly IScheduleRepository scheduleRepository;

    public GetUserMonthScheduleQueryHandler(IScheduleRepository scheduleRepository)
    {
        this.scheduleRepository = scheduleRepository;
    }

    public async Task<ScheduleDto> Handle(
        GetUserMonthScheduleQuery request,
        CancellationToken cancellationToken)
    {
        var period = new DateOnly(request.Year, request.Month, 1);
        var schedule = await scheduleRepository.GetAsync(
            request.UserId,
            request.DepartmentId,
            period,
            cancellationToken);

        schedule.EnsureExists("Invalid input");
        return schedule!.ToDto();
    }
}
