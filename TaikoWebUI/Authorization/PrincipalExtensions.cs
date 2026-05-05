using System.Security.Claims;
using TaikoLocalServer.Contracts.AdminApi.Authorization;

namespace TaikoWebUI.Authorization;

public static class PrincipalExtensions
{
    public static bool IsOwnerOrAdmin(this ClaimsPrincipal user, uint baid)
        => user.IsAdmin() || user.GetBaid() == baid;
}
