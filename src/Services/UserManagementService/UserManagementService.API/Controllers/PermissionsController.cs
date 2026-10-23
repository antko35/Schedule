namespace UserManagementService.API.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementService.API.Keycloak;
using UserManagementService.API.Keycloak.Models;

[Authorize]
[ApiController]
[Route("[controller]")]
public class PermissionsController : ControllerBase
{
    private readonly IKeycloakAdminService keycloakAdminService;

    public PermissionsController(IKeycloakAdminService keycloakAdminService)
    {
        this.keycloakAdminService = keycloakAdminService;
    }

    [HttpGet]
    [Route("GetClaims/{email}")]
    public async Task<IActionResult> GetClaims([FromRoute] string email)
    {
        var claims = await this.keycloakAdminService.GetUserClaimsAsync(email);
        return this.Ok(claims);
    }

    [HttpPost]
    [Route("AddClaim")]
    public async Task<IActionResult> AddClaim(ClaimRequest request)
    {
        await this.keycloakAdminService.AddClaimAsync(request);
        return this.Ok();
    }

    [HttpPost]
    [Route("DeleteClaim")]
    public async Task<IActionResult> DeleteClaim(ClaimRequest request)
    {
        await this.keycloakAdminService.DeleteClaimAsync(request);
        return this.Ok();
    }
}
