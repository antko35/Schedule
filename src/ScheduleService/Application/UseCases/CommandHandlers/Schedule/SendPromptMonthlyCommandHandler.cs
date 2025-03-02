using MediatR;
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

    public SendPromptMonthlyCommandHandler(
        IEmailService emailService,
        IScheduleRepository scheduleRepository,
        IUserRuleRepository scheduleRuleRepository)
    {
        this.emailService = emailService;
        this.scheduleRepository = scheduleRepository;
        this.scheduleRuleRepository = scheduleRuleRepository;
    }

    public async Task Handle(SendPromptMonthlyCommand request, CancellationToken cancellationToken)
    {
        var date = GetNextMonth();

        var departments = await GetDepartmentsWithEmptySchedule(date.year, date.month);

        Console.Out.WriteLine($"Sending prompt monthly: {departments.Count}");
        var emails = GetHeadEmails(departments);

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
            var body = $"{email.Username}, please fill schedule for {email.Clinic} for next month({month})";

            await emailService.SendEmailAsync(email.Email, "Schedule Generation", body);
        }
    }

    private List<EmailInfoDto> GetHeadEmails(List<string> departments)
    {
        var responce = new List<EmailInfoDto>();

        // получение из user management service с попмощью rabbit
        var email = "antkovking@gmail.com";
        var userName = "Anton Olegovich";
        var clinic1 = "Clinic ";
        var clinic2 = "Clinic 2";

        foreach (var department in departments)
        {
             EmailInfoDto info1 = new EmailInfoDto()
             {
                 Email = email,
                 Username = userName + " " + department,
                 Clinic = clinic1,
             };
             responce.Add(info1);
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

        public string Username { get; set; }

        public string Clinic { get; set; }

        public string Month { get; set; }
    }
}