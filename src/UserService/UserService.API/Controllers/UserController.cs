namespace UserService.API.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using UserService.Application.DTOs;
    using UserService.Domain.Constants;

    [ApiController]
    [Route("api/[controller]")]
    public class UserController(Application.Services.UserService userService) : ControllerBase
    {
        [HttpPost]
        [Route("shareAdminRole")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> ShareAdminRole([FromBody] ChangeUserRole request)
        {
            await userService.ChangeRole(request);

            return Ok();
        }
    }
}
