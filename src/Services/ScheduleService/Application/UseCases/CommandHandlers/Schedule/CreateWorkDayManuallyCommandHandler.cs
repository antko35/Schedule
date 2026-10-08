namespace ScheduleService.Application.UseCases.CommandHandlers.Schedule
{
    using System;
    using System.Threading.Tasks;
    using MediatR;
    using ScheduleService.Application.Services;
    using ScheduleService.Application.UseCases.Commands.Schedule;
    using ScheduleService.Domain.Abstractions;
    using ScheduleService.Domain.Models;

    public class CreateWorkDayManuallyCommandHandler
        : IRequestHandler<CreateWorkDayManuallyCommand, WorkDay>
    {
        private readonly IScheduleRepository scheduleRepository;
        private readonly ScheduleMonthContextService monthContextService;

        public CreateWorkDayManuallyCommandHandler(
            IScheduleRepository scheduleRepository,
            ScheduleMonthContextService monthContextService)
        {
            this.scheduleRepository = scheduleRepository;
            this.monthContextService = monthContextService;
        }

        public async Task<WorkDay> Handle(CreateWorkDayManuallyCommand request, CancellationToken cancellationToken)
        {
            var context = await this.monthContextService.GetOrCreateAsync(
                request.UserId,
                request.DepartmentId,
                request.StartTime.Year,
                request.StartTime.Month);
            var scheduleId = context.Rules.ScheduleId;

            var newWorkDay = new WorkDay
            {
                Day = request.StartTime.Day,
                StartTime = DateTime.SpecifyKind(request.StartTime, DateTimeKind.Utc),
                EndTime = DateTime.SpecifyKind(request.EndTime, DateTimeKind.Utc),
            };

            var isAlreadyExist = await IsExist(scheduleId, request.StartTime.Day);

            if (isAlreadyExist)
            {
                await scheduleRepository.UpdateWorkDayAsync(scheduleId, newWorkDay);
            }
            else
            {
                await scheduleRepository.AddWorkDayAsync(scheduleId, newWorkDay);
            }

            return newWorkDay;
        }

        private async Task<bool> IsExist(string scheduleId, int day)
        {
            var dailySchedule = await scheduleRepository.GetWorkDayAsync(scheduleId, day);
            return dailySchedule is not null;
        }
    }
}
