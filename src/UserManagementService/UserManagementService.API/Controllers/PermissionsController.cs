using Microsoft.AspNetCore.Mvc;
using UserManagementService.API.GrpcClient.Services;

namespace UserManagementService.API.Controllers;

[ApiController]
[Route("[controller]")]
public class PermissionsController : ControllerBase
{
    public readonly UserClient userClient;

    public PermissionsController(UserClient userClient)
    {
        this.userClient = userClient;
    }

    [HttpGet]
    [Route("GetClaims/{email}")]
    public async Task<IActionResult> GetClaims([FromRoute] string email)
    {
        var response = await userClient.GetUserClaimsAsync(email);
        return Ok(response);
    }

    [HttpPost]
    [Route("AddClaim")]
    public async Task AddClaim(AddClaimRequest request)
    {
        await userClient.AddClaimAsync(request);
    }

    [HttpPost]
    [Route("DeleteClaim")]
    public async Task DeleteClaim(AddClaimRequest request)
    {
        await userClient.DeleteClaimAsync(request);
    }
}
