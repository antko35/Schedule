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
        private readonly IUserRepository userRepository;
        private readonly IMediator mediator;

        public UserController(IUserRepository repository, IMediator mediator)
        {
            userRepository = repository;
            this.mediator = mediator;
        }

        /// <summary>
        /// Get all general users information
        /// </summary>
        /// <returns>List of users from collection 'Users'</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await mediator.Send(new GetAllUsersQuery());
            return Ok(users);
        }

        /// <summary>
        /// Get full user info by id.
        /// Contatins general info and info about working in department
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

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            await userRepository.AddAsync(user);
            return Ok();
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
