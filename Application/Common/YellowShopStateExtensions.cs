using TaikoLocalServer.Application.Catalog.Yellow;

namespace TaikoLocalServer.Application.Common;

public static class YellowShopStateExtensions
{
    public static async ValueTask<YellowShopSeasonState> GetOrCreateYellowShopSeasonStateAsync(
        this ITaikoDbContext context,
        UserSaveDataYellow saveData,
        uint seasonId,
        CancellationToken cancellationToken = default)
    {
        var existing = await context.YellowShopSeasonStates.FindAsync([saveData.Baid, seasonId], cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var hasAnyShopState = await context.YellowShopSeasonStates
            .AnyAsync(row => row.Baid == saveData.Baid, cancellationToken);
        var now = DateTime.UtcNow;
        var state = new YellowShopSeasonState
        {
            Baid = saveData.Baid,
            SeasonId = seasonId,
            TotalGetDonmedal = hasAnyShopState ? 0 : saveData.TotalGetDonmedal,
            TotalUseDonmedal = hasAnyShopState ? 0 : saveData.TotalUseDonmedal,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.YellowShopSeasonStates.Add(state);
        return state;
    }

    public static async ValueTask<YellowShopSeasonState?> GetOrCreateActiveYellowShopSeasonStateAsync(
        this ITaikoDbContext context,
        UserSaveDataYellow saveData,
        YellowItemShopCatalog itemShopCatalog,
        CancellationToken cancellationToken = default)
    {
        if (!itemShopCatalog.IsEnabled || itemShopCatalog.ActiveSeason is not { Items.Count: > 0 } activeSeason)
        {
            return null;
        }

        return await context.GetOrCreateYellowShopSeasonStateAsync(saveData, activeSeason.SeasonId, cancellationToken);
    }

    public static ValueTask<YellowShopSeasonState?> FindYellowShopSeasonStateAsync(
        this ITaikoDbContext context,
        uint baid,
        uint seasonId,
        CancellationToken cancellationToken = default)
        => context.YellowShopSeasonStates.FindAsync([baid, seasonId], cancellationToken);

    public static async Task<HashSet<(uint ItemType, uint ItemId)>> GetUnlockedYellowShopItemsAsync(
        this ITaikoDbContext context,
        uint baid,
        uint seasonId,
        CancellationToken cancellationToken = default)
    {
        var rows = await context.YellowShopItemStates
            .Where(row => row.Baid == baid
                && row.SeasonId == seasonId
                && row.Status == Ac15ShopItemStatus.Unlocked)
            .Select(row => new ValueTuple<uint, uint>(row.ItemType, row.ItemId))
            .ToListAsync(cancellationToken);

        return rows.ToHashSet();
    }
}
