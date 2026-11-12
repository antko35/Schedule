namespace ScheduleService.Application.UseCases.CommandHandlers.Schedule;

using MediatR;
using ScheduleService.Application.Services;
using ScheduleService.Application.UseCases.Commands.Schedule;
using ScheduleService.DataAccess.EmailSender;
using ScheduleService.Domain.Abstractions;

public sealed class SendPromptMonthlyCommandHandler : IRequestHandler<SendPromptMonthlyCommand>
{
    private readonly IEmailService emailService;
    private readonly IScheduleRepository scheduleRepository;
    private readonly UserEmailRpcService userEmailRpcService;

    public SendPromptMonthlyCommandHandler(
        IEmailService emailService,
        IScheduleRepository scheduleRepository,
        UserEmailRpcService userEmailRpcService)
    {
        this.emailService = emailService;
        this.scheduleRepository = scheduleRepository;
        this.userEmailRpcService = userEmailRpcService;
    }

    public async Task Handle(SendPromptMonthlyCommand request, CancellationToken cancellationToken)
    {
        var nextMonth = DateTime.UtcNow.AddMonths(1);
        var period = new DateOnly(nextMonth.Year, nextMonth.Month, 1);
        var emptySchedules = await scheduleRepository.GetEmptyAsync(period, cancellationToken);
        if (emptySchedules.Count == 0)
        {
            return;
        }

        var departmentIds = emptySchedules
            .Select(schedule => schedule.DepartmentId)
            .Distinct(StringComparer.Ordinal)
            .ToList();
        var emails = await userEmailRpcService.InvokeAsync(departmentIds, cancellationToken);
        var monthName = period.ToString("MMMM").ToLowerInvariant();

        foreach (var email in emails)
        {
            var body = $"{email}, please fill schedule for next month({monthName})";
            await emailService.SendEmailAsync(email, "Schedule Generation", body);
        }
    }
}
