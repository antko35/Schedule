namespace UserManagementService.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using UserManagementService.API.GrpcClient.Services;

[ApiController]
public class ClaimsController : ControllerBase
{
    private readonly UserClient userClient;

    public ClaimsController(UserClient userClient)
    {
        this.userClient = userClient;
    }

    /// <summary>
    /// Get all user permissions by email
    /// </summary>
    /// <param name="email">User email</param>
    /// <remarks>
    /// Call grpc method in user service
    /// </remarks>
    /// <returns></returns>
    [HttpGet]
    [Route("GetClaims/{email}")]
    public async Task<IActionResult> GetClaims([FromRoute] string email)
    {
        var response = await userClient.GetUserClaimsAsync(email);
        return Ok(response);
    }

    /// <summary>
    /// Add cliam (permission) to user by email
    /// </summary>
    /// <remarks>
    /// Call grpc method in user service.
    /// ```
    /// POST
    /// {
    ///     "email": "user@gmail.com", // users email
    ///     "claimType": "12GKP", // Clinic name
    ///     "claimValue": "UserManagment" // Claim (permission) name
    /// }
    /// ```
    /// </remarks>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("AddClaim")]
    public async Task AddClaim(AddClaimRequest request)
    {
        await userClient.AddClaimAsync(request);
    }

    /// <summary>
    /// Detete user claim (permission)
    /// </summary>
    /// <remarks>
    /// Call grpc method in user service.
    /// ```
    /// POST
    /// {
    ///     "email": "user@gmail.com", // users email
    ///     "claimType": "12GKP", // Clinic name
    ///     "claimValue": "UserManagment" // Claim (permission) name
    /// }
    /// ```
    /// </remarks>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("DeleteClaim")]
    public async Task DeleteClaim(AddClaimRequest request)
    {
        await userClient.DeleteClaimAsync(request);
    }
}
