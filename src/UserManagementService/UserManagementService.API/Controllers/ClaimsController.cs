namespace UserManagementService.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using UserManagementService.API.Keycloak;
using UserManagementService.API.Keycloak.Models;

[ApiController]
public class ClaimsController : ControllerBase
{
    private readonly IKeycloakAdminService keycloakAdminService;

    public ClaimsController(IKeycloakAdminService keycloakAdminService)
    {
        this.keycloakAdminService = keycloakAdminService;
    }

    /// <summary>
    /// Get all user attributes (claims) by email
    /// </summary>
    /// <param name="email">User email</param>
    [HttpGet]
    [Route("GetClaims/{email}")]
    public async Task<IActionResult> GetClaims([FromRoute] string email)
    {
        var claims = await this.keycloakAdminService.GetUserClaimsAsync(email);
        return this.Ok(claims);
    }

    /// <summary>
    /// Add a claim (attribute) to a Keycloak user
    /// </summary>
    /// <remarks>
    /// ```
    /// POST
    /// {
    ///     "email": "user@gmail.com",
    ///     "claimType": "12GKP",
    ///     "claimValue": "UserManagement"
    /// }
    /// ```
    /// </remarks>
    [HttpPost]
    [Route("AddClaim")]
    public async Task<IActionResult> AddClaim(ClaimRequest request)
    {
        await this.keycloakAdminService.AddClaimAsync(request);
        return this.Ok();
    }

    /// <summary>
    /// Delete a claim (attribute) from a Keycloak user
    /// </summary>
    [HttpPost]
    [Route("DeleteClaim")]
    public async Task<IActionResult> DeleteClaim(ClaimRequest request)
    {
        await this.keycloakAdminService.DeleteClaimAsync(request);
        return this.Ok();
    }
}
