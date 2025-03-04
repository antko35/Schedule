using MediatR;
using ScheduleService.Application.Services;
using ScheduleService.Application.UseCases.Commands.Schedule;
using ScheduleService.DataAccess.EmailSender;
using ScheduleService.DataAccess.Repository;
using ScheduleService.Domain.Abstractions;

namespace ScheduleService.Application.UseCases.CommandHandlers.Schedule;

public class SendPromptMonthlyCommandHandler
    : IRequestHandler<SendPromptMonthlyCommand>
{
    private readonly IEmailService emailService;
    private readonly IScheduleRepository scheduleRepository;
    private readonly IUserRuleRepository scheduleRuleRepository;
    private readonly RpcService rpcService;

    public SendPromptMonthlyCommandHandler(
        IEmailService emailService,
        IScheduleRepository scheduleRepository,
        IUserRuleRepository scheduleRuleRepository,
        RpcService rpcService)
    {
        this.emailService = emailService;
        this.scheduleRepository = scheduleRepository;
        this.scheduleRuleRepository = scheduleRuleRepository;
        this.rpcService = rpcService;
    }

    public async Task Handle(SendPromptMonthlyCommand request, CancellationToken cancellationToken)
    {
        var date = GetNextMonth();

        var departments = await GetDepartmentsWithEmptySchedule(date.year, date.month);

        var emails = await GetHeadEmails(departments);

        await SendEmails(emails, date.month);
    }

    private (int year, string month) GetNextMonth()
    {
        var date = DateTime.Now.AddMonths(1);
        var year = date.Year;
        var nextMonthName = date.ToString("MMMM").ToLower();

        return (year, nextMonthName);
    }

    private async Task SendEmails(List<EmailInfoDto> emails, string month)
    {
        foreach (var email in emails)
        {
            var body = $"{email.Email}, please fill schedule for next month({month})";

            await emailService.SendEmailAsync(email.Email, "Schedule Generation", body);
        }
    }

    private async Task<List<EmailInfoDto>> GetHeadEmails(List<string> departments)
    {
        Console.Out.WriteLine(departments.First());
        
        var emails = await rpcService.InvokeAsync(departments);

        Console.Out.WriteLine(emails.First());
        Console.Out.WriteLine("-----------");
        var responce = new List<EmailInfoDto>();
        foreach (var email in emails)
        {
             EmailInfoDto info = new EmailInfoDto()
             {
                 Email = email,
             };
             responce.Add(info);
        }

        return responce;
    }

    private async Task<List<string>> GetDepartmentsWithEmptySchedule(int year, string month)
    {
        var emptySchedules = await scheduleRepository.GetEmptySchedules(year, month);

        if (emptySchedules.Count == 0)
        {
            Console.Out.WriteLine($"No empty schedules for year {year}, month {month}");
            return new List<string>();
        }

        Console.Out.WriteLine($"Empty schedules count: {emptySchedules.Count}");
        var departmentsId = new List<string>();
        foreach (var emptySchedule in emptySchedules)
        {
            var departmentId = await scheduleRuleRepository.GetIdByScheduleId(emptySchedule.Id!);

            departmentsId.Add(departmentId);
        }

        return departmentsId;
    }

    private class EmailInfoDto
    {
        public string Email { get; set; }
    }
}