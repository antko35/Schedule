namespace ScheduleService.Application.UseCases.CommandHandlers.Schedule
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using MediatR;
    using ScheduleService.Application.UseCases.Commands.Schedule;
    using ScheduleService.DataAccess.Repository;

    public class DeleteWorkDayCommandHandler : IRequestHandler<DeleteWorkDayCommand>
    {
        private readonly IUserRuleRepository userRuleRepository;

        public DeleteWorkDayCommandHandler(IUserRuleRepository userRuleRepository)
        {
            this.userRuleRepository = userRuleRepository;
        }

        public async Task Handle(DeleteWorkDayCommand request, CancellationToken cancellationToken)
        {
            string monthName = new DateTime(request.WorkDay.Year, request.WorkDay.Month, request.WorkDay.Day).ToString("MMMM", CultureInfo.CreateSpecificCulture("es"));

            var schedule = await userRuleRepository.GetMonthScheduleRules(request.UserId, request.DepartmentId, monthName);

            var dayToDelete = schedule.Schedule.Find(x => x.StartTime.Day == request.WorkDay.Day);
            if (dayToDelete != null)
            {
                await userRuleRepository.DeleteWorkDayAsync(request.UserId, request.DepartmentId, monthName, request.WorkDay);
            }
            else
            {
                throw new InvalidOperationException("Work in this day not found");
            }
        }
    }
}
