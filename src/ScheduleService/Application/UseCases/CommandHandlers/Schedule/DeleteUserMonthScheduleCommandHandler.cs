using MediatR;
using ScheduleService.Application.Extensions;
using ScheduleService.Application.UseCases.Commands.Schedule;
using ScheduleService.DataAccess.Repository;
using ScheduleService.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleService.Application.UseCases.CommandHandlers.Schedule
{
    public class DeleteUserMonthScheduleCommandHandler
        : IRequestHandler<DeleteUserMonthScheduleCommand, string>
    {
        private readonly IScheduleRepository scheduleRepository;
        private readonly IUserRuleRepository userRuleRepository;

        public DeleteUserMonthScheduleCommandHandler(
            IScheduleRepository scheduleRepository,
            IUserRuleRepository userRuleRepository)
        {
            this.scheduleRepository = scheduleRepository;
            this.userRuleRepository = userRuleRepository;
        }

        public async Task<string> Handle(DeleteUserMonthScheduleCommand request, CancellationToken cancellationToken)
        {
            string response;

            var scheduleId = await GetUsersScheduleId(request);

            var result = await scheduleRepository.ClearMonthSchedule(scheduleId);

            if (result.ModifiedCount > 0)
            {
                response = $" Schedule {scheduleId} was cleaned.";
                return response;
            }
            else
            {
                response = "Nothing to delete";
                return response;
            }
        }

        private async Task<string> GetUsersScheduleId(DeleteUserMonthScheduleCommand request)
        {
            string monthName = new DateTime(request.Year, request.Month, 1)
                .ToString("MMMM")
                .ToLower();

            var userRules = await userRuleRepository.GetMonthScheduleRules(request.UserId, request.DepartmentId, monthName, request.Year);

            userRules.EnsureExists("Invalid input");
            var scheduleId = userRules.ScheduleId;

            return scheduleId;
        }
    }
}
