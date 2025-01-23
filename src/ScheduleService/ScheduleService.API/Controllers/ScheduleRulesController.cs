using MediatR;
using Microsoft.AspNetCore.Mvc;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;

namespace ScheduleService.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ScheduleRulesController : ControllerBase
    {
        private readonly IMediator mediator;

        public ScheduleRulesController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost]
        public async Task CreateUserRules([FromBody] CreateScheduleRulesCommand command)
        {
            await mediator.Send(command);
        }
    }
}
