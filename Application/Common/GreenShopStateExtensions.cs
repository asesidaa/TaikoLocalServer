namespace TaikoLocalServer.Application.Common;

public static class GreenShopStateExtensions
{
    public static async ValueTask<GreenShopSeasonState> GetOrCreateGreenShopSeasonStateAsync(
        this ITaikoDbContext context,
        UserSaveDataGreen saveData,
        uint seasonId,
        CancellationToken cancellationToken = default)
    {
        var existing = await context.GreenShopSeasonStates.FindAsync([saveData.Baid, seasonId], cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var hasAnyShopState = await context.GreenShopSeasonStates
            .AnyAsync(row => row.Baid == saveData.Baid, cancellationToken);
        var now = DateTime.UtcNow;
        var state = new GreenShopSeasonState
        {
            Baid = saveData.Baid,
            SeasonId = seasonId,
            TotalGetDonmedal = hasAnyShopState ? 0 : saveData.TotalGetDonmedal,
            TotalUseDonmedal = hasAnyShopState ? 0 : saveData.TotalUseDonmedal,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.GreenShopSeasonStates.Add(state);
        return state;
    }
}
