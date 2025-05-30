namespace UserManagementService.API.Controllers
{
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using UserManagementService.Application.UseCases.Commands.User;
    using UserManagementService.Application.UseCases.Queries.User;
    using UserManagementService.Domain.Abstractions.IRepository;
    using UserManagementService.Domain.Models;

    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IMediator mediator;

        public UserController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        /// <summary>
        /// Get general informationfor all users
        /// </summary>
        /// <returns>List of users from collection 'Users'</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await mediator.Send(new GetAllUsersQuery());

            return Ok(users);
        }

        /// <summary>
        /// Get list of users(id, name, surname)
        /// </summary>
        /// <returns>List of users from collection 'Users'</returns>
        [HttpGet]
        [Route("ShortUserInfo")]
        public async Task<IActionResult> GetUsersList()
        {
            var users = await mediator.Send(new GetUsersListQuery());

            return Ok(users);
        }

        /// <summary>
        /// Get full user info by id.
        /// Contatins general info and info about work in departments
        /// </summary>
        /// <param name="id">User Id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var response = await mediator.Send(new GetFullUserInfoQuery(id));

            return Ok(response);
        }

        /// <summary>
        /// Create user
        /// </summary>
        /// <param name="command"></param>
        /// <returns>created user</returns>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
        {
            var result = await mediator.Send(command);

            return Ok(result);
        }

        /// <summary>
        /// Update information about existing user
        /// </summary>
        /// <param name="command"></param>
        /// <returns>updated user</returns>
        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserInfoCommand command)
        {
            var result = await mediator.Send(command);

            return Ok(result);
        }

        /// <summary>
        /// Delete User and userJobs
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            await mediator.Send(new DeleteUserCommand(id));

            return Ok();
        }
    }
}
