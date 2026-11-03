namespace TaikoLocalServer.Adapters.AdminApi.Authorization;

/// <summary>The language a player's song titles show in (MusicDetail.Title): Settings › Display's choice, else their
/// profile's Language.</summary>
public static class SongLanguage
{
    public static async Task<uint> ForAsync(ITaikoDbContext context, uint baid, CancellationToken ct) =>
        await context.UserWebProfiles.Where(p => p.Baid == baid).Select(p => p.SongLanguage).FirstOrDefaultAsync(ct)
        ?? await context.UserData.Where(u => u.Baid == baid).Select(u => u.MyDonNameLanguage).FirstOrDefaultAsync(ct);
}
