namespace UserManagementService.API.Controllers
{
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using UserManagementService.Application.UseCases.Commands.Department;
    using UserManagementService.Application.UseCases.Queries.Department;
    using UserManagementService.Domain.Abstractions.IRepository;
    using UserManagementService.Domain.Models;

    [ApiController]
    [Route("[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly IMediator mediator;

        public DepartmentController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDepartments()
        {
            var users = await mediator.Send(new GetAllDepartmentsQuery());
            return Ok(users);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await mediator.Send(new GetDepartmentByIdQuery(id));
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] Department department)
        {
            var createdDep = await mediator.Send(new CreateDepartmentCommand(department));
            return Ok(createdDep);
        }

        [HttpPost]
        [Route("addUserToDepartment")]
        //[Route("addUserToDepartment/{userId}/{departmentId}")]
        //public async Task<IActionResult> AddUserToDepartment(string userId, string departmentId)
        public async Task<IActionResult> AddUserToDepartment([FromBody] UserJob userJob)
        {
            await mediator.Send(new AddUserToDepartmentCommand(userJob));
            return Ok();
        }

    }
}
