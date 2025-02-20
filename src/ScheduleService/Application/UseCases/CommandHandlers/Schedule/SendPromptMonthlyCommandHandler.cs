using MediatR;
using ScheduleService.Application.UseCases.Commands.Schedule;
using ScheduleService.DataAccess.EmailSender;

namespace ScheduleService.Application.UseCases.CommandHandlers.Schedule;

public class SendPromptMonthlyCommandHandler
    : IRequestHandler<SendPromptMonthlyCommand>
{
    private readonly IEmailService emailService;

    public SendPromptMonthlyCommandHandler(IEmailService emailService)
    {
        this.emailService = emailService;
    }

    public async Task Handle(SendPromptMonthlyCommand request, CancellationToken cancellationToken)
    {
        // получение из user management service с попмощью rabbit
        var email = "antkovking@gmail.com";
        var userName = "Anton Olegovich";
        var clinic = "Clinic num";
        var month = "January";
        var body = $"{userName}, please fill schedule for {clinic} for next month({month})";
        
        await emailService.SendEmailAsync(email, "Schedule Generation", body);
    }
}