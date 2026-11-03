using System.Security.Claims;

namespace TaikoLocalServer.Adapters.AdminApi.Authorization;

/// <summary>The language song titles show in (MusicDetail.Title) for whoever is looking: the signed-in player (else, in
/// Local mode, the page's player): their Settings › Display › Song titles, else their profile's Language.</summary>
public static class SongLanguage
{
    public static async Task<uint> ForAsync(ITaikoDbContext context, ClaimsPrincipal viewer, uint baid, CancellationToken ct)
    {
        var who = viewer.GetBaid() ?? baid;
        return await context.UserWebProfiles.Where(p => p.Baid == who).Select(p => p.SongLanguage).FirstOrDefaultAsync(ct)
            ?? await context.UserData.Where(u => u.Baid == who).Select(u => u.MyDonNameLanguage).FirstOrDefaultAsync(ct);
    }
}
