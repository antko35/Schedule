namespace ScheduleService.Application.UseCases.CommandHandlers.Schedule
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using MediatR;
    using ScheduleService.Application.UseCases.Commands.Schedule;
    using ScheduleService.DataAccess.Repository;
    using ScheduleService.Domain.Models;

    public class CreateWorkDayManuallyCommandHandler : IRequestHandler<CreateWorkDayManuallyCommand>
    {
        private readonly IUserRuleRepository userRuleRepository;

        public CreateWorkDayManuallyCommandHandler(IUserRuleRepository userRuleRepository)
        {
            this.userRuleRepository = userRuleRepository;
        }

        public async Task Handle(CreateWorkDayManuallyCommand request, CancellationToken cancellationToken)
        {
            string monthName = new DateTime(request.StartTime.Year, request.StartTime.Month, 1).ToString("MMMM", CultureInfo.CreateSpecificCulture("es"));

            var workDaySchedue = await userRuleRepository.GetWorkDaySchedue(request.UserId, request.DepartmentId, monthName);

            var newWorkDay = new WorkDay
            {
                StartTime = request.StartTime,
                EndTime = request.EndTime,
            };

            var workDay = workDaySchedue.Schedule.Where(x => x.StartTime.Day == request.StartTime.Day);

            if (workDay.FirstOrDefault() != null)
            {
                await userRuleRepository.UpdateWorkDayAsync(request.UserId, request.DepartmentId, monthName, newWorkDay);
            }
            else
            {
                await userRuleRepository.AddWorkDayAsync(request.UserId, request.DepartmentId, monthName, newWorkDay);
            }
        }
    }
}
