namespace UserManagementService.API.Keycloak;

using UserManagementService.API.Keycloak.Models;

public interface IKeycloakAdminService
{
    Task<IReadOnlyDictionary<string, string>> GetUserClaimsAsync(string email);

    Task AddClaimAsync(ClaimRequest request);

    Task DeleteClaimAsync(ClaimRequest request);
}
