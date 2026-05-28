using TaikoLocalServer.Application.Catalog.Blue;

namespace TaikoLocalServer.Application.Common;

public static class BlueShopStateExtensions
{
    public static async ValueTask<BlueShopSeasonState> GetOrCreateBlueShopSeasonStateAsync(
        this ITaikoDbContext context,
        UserSaveDataBlue saveData,
        uint seasonId,
        CancellationToken cancellationToken = default)
    {
        var existing = await context.BlueShopSeasonStates.FindAsync([saveData.Baid, seasonId], cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var now = DateTime.UtcNow;
        var state = new BlueShopSeasonState
        {
            Baid = saveData.Baid,
            SeasonId = seasonId,
            TotalGetDonmedal = 0,
            TotalUseDonmedal = 0,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.BlueShopSeasonStates.Add(state);
        return state;
    }

    public static async ValueTask<BlueShopSeasonState?> GetOrCreateActiveBlueShopSeasonStateAsync(
        this ITaikoDbContext context,
        UserSaveDataBlue saveData,
        BlueItemShopCatalog itemShopCatalog,
        CancellationToken cancellationToken = default)
    {
        if (!itemShopCatalog.IsEnabled || itemShopCatalog.ActiveSeason is not { Items.Count: > 0 } activeSeason)
        {
            return null;
        }

        return await context.GetOrCreateBlueShopSeasonStateAsync(saveData, activeSeason.SeasonId, cancellationToken);
    }

    public static ValueTask<BlueShopSeasonState?> FindBlueShopSeasonStateAsync(
        this ITaikoDbContext context,
        uint baid,
        uint seasonId,
        CancellationToken cancellationToken = default)
        => context.BlueShopSeasonStates.FindAsync([baid, seasonId], cancellationToken);

    public static async Task<HashSet<(uint ItemType, uint ItemId)>> GetUnlockedBlueShopItemsAsync(
        this ITaikoDbContext context,
        uint baid,
        uint seasonId,
        CancellationToken cancellationToken = default)
    {
        var rows = await context.BlueShopItemStates
            .Where(row => row.Baid == baid
                && row.SeasonId == seasonId
                && row.Status == BlueShopItemStatus.Unlocked)
            .Select(row => new ValueTuple<uint, uint>(row.ItemType, row.ItemId))
            .ToListAsync(cancellationToken);

        return rows.ToHashSet();
    }
}
