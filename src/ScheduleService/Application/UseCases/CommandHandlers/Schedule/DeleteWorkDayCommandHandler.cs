namespace ScheduleService.Application.UseCases.CommandHandlers.Schedule
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using MediatR;
    using ScheduleService.Application.UseCases.Commands.Schedule;
    using ScheduleService.DataAccess.Repository;
    using ScheduleService.Domain.Abstractions;
    using ScheduleService.Domain.Models;

    public class DeleteWorkDayCommandHandler
        : IRequestHandler<DeleteWorkDayCommand, Schedule>
    {
        private readonly IUserRuleRepository userRuleRepository;
        private readonly IScheduleRepository scheduleRepository;

        public DeleteWorkDayCommandHandler(
            IUserRuleRepository userRuleRepository,
            IScheduleRepository scheduleRepository)
        {
            this.userRuleRepository = userRuleRepository;
            this.scheduleRepository = scheduleRepository;
        }

        public async Task<Schedule> Handle(DeleteWorkDayCommand request, CancellationToken cancellationToken)
        {
            string monthName = new DateTime(request.WorkDay.Year, request.WorkDay.Month, request.WorkDay.Day)
                .ToString("MMMM")
                .ToLower();

            var scheduleRules = await userRuleRepository
                .GetMonthScheduleRules(request.UserId, request.DepartmentId, monthName, request.WorkDay.Year)
                ?? throw new KeyNotFoundException("Schedule for this user not found");

            var scheduleForDay = await scheduleRepository
                .GetWorkDayAsync(scheduleRules.ScheduleId, request.WorkDay.Day)
                ?? throw new InvalidOperationException("No work during day");

            await scheduleRepository.DeleteWorkDayAsync(scheduleRules.ScheduleId, request.WorkDay.Day);

            return scheduleForDay;
        }
    }
}
