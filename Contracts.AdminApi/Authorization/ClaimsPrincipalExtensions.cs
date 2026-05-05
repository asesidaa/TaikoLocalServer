using System.Security.Claims;

namespace TaikoLocalServer.Contracts.AdminApi.Authorization;

public static class ClaimsPrincipalExtensions
{
    public static uint? GetBaid(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(ClaimTypes.Name)?.Value;
        return uint.TryParse(value, out var baid) ? baid : null;
    }

    public static bool IsAdmin(this ClaimsPrincipal user) => user.IsInRole(AuthPolicies.Admin);
}
