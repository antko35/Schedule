using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using ScheduleService.DataAccess.EmailSender;

namespace ScheduleService.Infrastructure.EmailSender;

public class SmtpEmailService : IEmailService
{
    private readonly SmtpSettings smtpSettings;

    public SmtpEmailService(IOptions<SmtpSettings> smtpSettings)
    {
        this.smtpSettings = smtpSettings.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        using var smtpClient = new SmtpClient(smtpSettings.Host, smtpSettings.Port)
        {
            Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(smtpSettings.Username),
            Subject = subject,
            Body = body,
            IsBodyHtml = true,
        };

        mailMessage.To.Add(to);
        await smtpClient.SendMailAsync(mailMessage);
    }
}