namespace ScheduleService.DataAccess.EmailSender;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}