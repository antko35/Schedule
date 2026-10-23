using MediatR;
using ScheduleService.Application.Services;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;

namespace ScheduleService.Application.UseCases.CommandHandlers.ScheduleRules;

public class CreateRulesForSpecificMonthCommandHandler : IRequestHandler<CreateRulesForSpecificMonthCommand>
{
    private ScheduleRulesGenerator scheduleRulesGenerator;

    public CreateRulesForSpecificMonthCommandHandler(ScheduleRulesGenerator scheduleRulesGenerator)
    {
        this.scheduleRulesGenerator = scheduleRulesGenerator;
    }

    public async Task Handle(CreateRulesForSpecificMonthCommand request, CancellationToken cancellationToken)
    {
        var departmentId = request.DepartmentId;

        var month = request.Month;
        var year = request.Year;

            //var users = 
        //await scheduleRulesGenerator.CreateAndAddRulesAndSchedules(request.UserId, departmentId, new DateTime(year, month, 1), month);
    }
}