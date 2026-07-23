namespace UserManagementService.API.Keycloak.Models;

public record ClaimRequest(string Email, string ClaimType, string ClaimValue);
