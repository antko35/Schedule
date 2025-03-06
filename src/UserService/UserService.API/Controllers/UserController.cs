namespace UserService.API.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using UserService.Application.DTOs;
    using UserService.Domain.Constants;

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController(Application.Services.UserService userService) : ControllerBase
    {
        [HttpGet]
        [Route("getPermissions")]
        public async Task<IActionResult> GetPermissions()
        {
            var claims = User.Claims;

            var claimList = claims.Select(c => new
            {
                Type = c.Type,
                Value = c.Value
            }).ToList();

            return Ok(claimList);
        }

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
