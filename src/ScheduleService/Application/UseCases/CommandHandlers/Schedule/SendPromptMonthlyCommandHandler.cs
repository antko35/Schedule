using MediatR;
using ScheduleService.Application.UseCases.Commands.Schedule;
using ScheduleService.DataAccess.EmailSender;
using ScheduleService.Domain.Abstractions;

namespace ScheduleService.Application.UseCases.CommandHandlers.Schedule;

public class SendPromptMonthlyCommandHandler
    : IRequestHandler<SendPromptMonthlyCommand>
{
    private readonly IEmailService emailService;
    private readonly IScheduleRepository scheduleRepository;

    public SendPromptMonthlyCommandHandler(
        IEmailService emailService,
        IScheduleRepository scheduleRepository)
    {
        this.emailService = emailService;
        this.scheduleRepository = scheduleRepository;
    }

    public async Task Handle(SendPromptMonthlyCommand request, CancellationToken cancellationToken)
    {
        var departments = await GetDepartmentsWithEmptySchedule();

        var emails = GetHeadEmails(departments);

        await SendEmails(emails);
    }

    private async Task SendEmails(List<EmailInfoDto> emails)
    {
        foreach (var email in emails)
        {
            var body = $"{email.Username}, please fill schedule for {email.Clinic} for next month({email.Month})";

            await emailService.SendEmailAsync(email.Email, "Schedule Generation", body);
        }
    }

    private List<EmailInfoDto> GetHeadEmails(List<string> departments)
    {
        // получение из user management service с попмощью rabbit
        var email = "antkovking@gmail.com";
        var userName = "Anton Olegovich";
        var clinic = "Clinic num";
        var month = "January";

        var responce = new List<EmailInfoDto>();

        return responce;
    }

    private async Task<List<string>> GetDepartmentsWithEmptySchedule()
    {
        var departments = new List<string>();
        return departments;
    }

    private class EmailInfoDto
    {
        public string Email { get; set; }

        public string Username { get; set; }

        public string Clinic { get; set; }

        public string Month { get; set; }
    }
}