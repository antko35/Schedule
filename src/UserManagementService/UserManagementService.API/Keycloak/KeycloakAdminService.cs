namespace UserManagementService.API.Keycloak;

using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using UserManagementService.API.Keycloak.Models;

public class KeycloakAdminService : IKeycloakAdminService
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly string realm;
    private readonly string adminUser;
    private readonly string adminPassword;

    public KeycloakAdminService(IHttpClientFactory factory, IConfiguration configuration)
    {
        this.httpClientFactory = factory;
        this.realm = configuration["Keycloak:Realm"] ?? "schedule-service";
        this.adminUser = configuration["Keycloak:AdminUser"] ?? "admin";
        this.adminPassword = configuration["Keycloak:AdminPassword"] ?? "admin";
    }

    public async Task<IReadOnlyDictionary<string, string>> GetUserClaimsAsync(string email)
    {
        var client = await this.CreateAuthorizedClientAsync();
        var userId = await this.GetUserIdAsync(client, email);

        var response = await client.GetAsync($"/admin/realms/{this.realm}/users/{userId}");
        response.EnsureSuccessStatusCode();

        var user = await response.Content.ReadFromJsonAsync<KeycloakUser>();
        return user?.Attributes?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.FirstOrDefault() ?? string.Empty)
               ?? new Dictionary<string, string>();
    }

    public async Task AddClaimAsync(ClaimRequest request)
    {
        var client = await this.CreateAuthorizedClientAsync();
        var userId = await this.GetUserIdAsync(client, request.Email);

        var getResponse = await client.GetAsync($"/admin/realms/{this.realm}/users/{userId}");
        getResponse.EnsureSuccessStatusCode();

        var user = await getResponse.Content.ReadFromJsonAsync<KeycloakUser>();
        var attributes = user?.Attributes ?? new Dictionary<string, List<string>>();
        attributes[request.ClaimType] = [request.ClaimValue];

        var putResponse = await client.PutAsJsonAsync(
            $"/admin/realms/{this.realm}/users/{userId}",
            new { attributes });
        putResponse.EnsureSuccessStatusCode();
    }

    public async Task DeleteClaimAsync(ClaimRequest request)
    {
        var client = await this.CreateAuthorizedClientAsync();
        var userId = await this.GetUserIdAsync(client, request.Email);

        var getResponse = await client.GetAsync($"/admin/realms/{this.realm}/users/{userId}");
        getResponse.EnsureSuccessStatusCode();

        var user = await getResponse.Content.ReadFromJsonAsync<KeycloakUser>();
        var attributes = user?.Attributes ?? new Dictionary<string, List<string>>();
        attributes.Remove(request.ClaimType);

        var putResponse = await client.PutAsJsonAsync(
            $"/admin/realms/{this.realm}/users/{userId}",
            new { attributes });
        putResponse.EnsureSuccessStatusCode();
    }

    private async Task<HttpClient> CreateAuthorizedClientAsync()
    {
        var client = this.httpClientFactory.CreateClient("keycloak-admin");
        var token = await this.GetAdminTokenAsync(client);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private async Task<string> GetAdminTokenAsync(HttpClient client)
    {
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = "admin-cli",
            ["username"] = this.adminUser,
            ["password"] = this.adminPassword,
        };

        var response = await client.PostAsync(
            "/realms/master/protocol/openid-connect/token",
            new FormUrlEncodedContent(form));
        response.EnsureSuccessStatusCode();

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return doc.RootElement.GetProperty("access_token").GetString()!;
    }

    private async Task<string> GetUserIdAsync(HttpClient client, string email)
    {
        var response = await client.GetAsync(
            $"/admin/realms/{this.realm}/users?email={Uri.EscapeDataString(email)}&exact=true");
        response.EnsureSuccessStatusCode();

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var users = doc.RootElement.EnumerateArray().ToList();
        if (users.Count == 0)
        {
            throw new InvalidOperationException($"Keycloak user not found: {email}");
        }

        return users[0].GetProperty("id").GetString()!;
    }

    private sealed class KeycloakUser
    {
        [JsonPropertyName("attributes")]
        public Dictionary<string, List<string>>? Attributes { get; set; }
    }
}
