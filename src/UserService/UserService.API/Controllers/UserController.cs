namespace UserService.API.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using UserService.Application.DTOs;
    using UserService.Domain.Constants;

    [ApiController]
    [Route("[controller]")]
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

            var permissions = claimList.Select(x => x.Value).ToList();

            var resp = string.Join(",", permissions);

            Response.Headers.Append("X-User-Permissions", resp);

            return Ok();
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
