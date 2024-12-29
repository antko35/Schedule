namespace UserService.Infrastructure.EmailSender
{
    using System.Net;
    using System.Net.Mail;
    using Microsoft.AspNetCore.Identity.UI.Services;
    using Microsoft.Extensions.Configuration;

    internal class IdentityEmailSender : IEmailSender
    {
        private readonly IConfiguration configuration;

        public IdentityEmailSender(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var mailMessage = new MailMessage
            {
               // From = new MailAddress(configuration["Smtp:From"]),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true,
                From = new MailAddress(configuration["Smtp:From"]),
            };
            mailMessage.To.Add(email);

            using var smtpClient = new SmtpClient(configuration["Smtp:Host"], int.Parse(configuration["Smtp:Port"]));
            smtpClient.EnableSsl = bool.Parse(configuration["Smtp:EnableSsl"]);
            smtpClient.Credentials = new NetworkCredential(configuration["Smtp:Username"], configuration["Smtp:Password"]);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
