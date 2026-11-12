namespace UserManagementService.Domain.Models;

using System.Security.Cryptography;

public static class EntityId
{
    public static string Normalize(string value) => value.ToLowerInvariant();

    public static string New() => Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(12));
}
