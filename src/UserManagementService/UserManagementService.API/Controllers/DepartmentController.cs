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

        /// <summary>
        /// Get all departments from all clinics
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAllDepartments()
        {
            var users = await mediator.Send(new GetAllDepartmentsQuery());
            return Ok(users);
        }

        /// <summary>
        /// Get department by its own id
        /// </summary>
        /// <param name="id">Department id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await mediator.Send(new GetDepartmentByIdQuery(id));
            return Ok(result);
        }

        /// <summary>
        /// Create department
        /// </summary>
        /// <param name="command"></param>
        /// <remarks>
        /// ```
        /// POST
        /// {
        ///     "departmentName": "stomatology",
        ///     "clinicId": "" // if null - auto generating, unknown
        /// }
        /// ```
        /// </remarks>
        /// <returns>Created department</returns>
        [HttpPost]
        public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentCommand command)
        {
            var createdDep = await mediator.Send(command);
            return Ok(createdDep);
        }

        /// <summary>
        /// Get all users form department
        /// </summary>
        /// <param name="deparmtmentId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("getUsersByDepartment/{deparmtmentId}")]
        public async Task<IActionResult> GetUsersByDepartment(string deparmtmentId)
        {
            var users = await mediator.Send(new GetUsersByDepartmentQuery(deparmtmentId));
            return Ok(users);
        }

        /// <summary>
        /// Add user to department
        /// </summary>
        /// <param name="command"></param>
        /// <remarks>
        /// ```
        /// POST
        /// {
        ///   "userId": "",
        ///   "departmentId": "",
        ///   "role": "string", // role in this dep
        ///   "status": "string" // working / vacation
        ///   "email": "user@gmail.com",
        ///   "phoneNumber": "+375-12-3456789"
        /// }
        /// ```
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        [Route("addUserToDepartment")]
         public async Task<IActionResult> AddUserToDepartment([FromBody] AddUserToDepartmentCommand command)
         {
            await mediator.Send(command);
            return Ok();
         }

        /// <summary>
        /// Edit information about user in department
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("editUserInDepartment")]
        public async Task<IActionResult> EditUserInDepartment([FromBody] EditUserInDepartmentCommand command)
        {
            await mediator.Send(command);

            return Ok();
        }

        /// <summary>
        /// Remove user from department.
        /// </summary>
        /// <remarks></remarks>
        /// <param name="userId">User userId that you want to remove from department</param>
        /// <param name="departmentId">Department Id from which you want to remove</param>
        [HttpDelete]
        [Route("deleteFromDepartment/{userId}/{departmentId}")]
        public async Task<IActionResult> RemoveUserFromDepartment(string userId, string departmentId)
        {
            var deletedUserJob = await mediator.Send(new RemoveUserFromDepartmentCommand(userId, departmentId));
            return Ok(deletedUserJob);
        }
    }
}
