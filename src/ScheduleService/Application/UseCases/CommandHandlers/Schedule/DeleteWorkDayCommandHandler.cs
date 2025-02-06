namespace ScheduleService.Application.UseCases.CommandHandlers.Schedule
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using MediatR;
    using ScheduleService.Application.Extensions;
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
            var scheduleId = await GetScheduleId(request);

            var dayToDelete = request.WorkDay.Day;

            var workDayToDelete = await GetDayToDelete(scheduleId, dayToDelete);

            workDayToDelete.EnsureExists("No work during day");

            await scheduleRepository.DeleteWorkDayAsync(scheduleId, dayToDelete);

            return workDayToDelete;
        }

        private async Task<Schedule?> GetDayToDelete(string scheduleId, int day)
        {
            var scheduleForDay = await scheduleRepository
                 .GetWorkDayAsync(scheduleId, day);

            return scheduleForDay;
        }

        private async Task<string> GetScheduleId(DeleteWorkDayCommand request)
        {
            string monthName = new DateTime(request.WorkDay.Year, request.WorkDay.Month, request.WorkDay.Day)
                .ToString("MMMM")
                .ToLower();

            var scheduleRules = await userRuleRepository
                .GetMonthScheduleRules(request.UserId, request.DepartmentId, monthName, request.WorkDay.Year);

            scheduleRules.EnsureExists("Schedule for this user not found");

            return scheduleRules.ScheduleId;
        }
    }
}
