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
        [Route("shareAdmin")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> ShareAdmin([FromBody] ChangeUserRole request)
        {
            await userService.ChangeRole(request);
            return Ok();
        }

        [HttpPost]
        [Route("addClaim")]
        //[Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> addClaim([FromBody] AddClaimRequest request)
        {
            var result = await userService.AddClaimToPerson(request);
            return Ok(result);
        }
    }
}
