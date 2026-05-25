namespace TaikoLocalServer.Application.Handlers;

public partial class RewardExecutionCommandHandler
{
    public partial async ValueTask<CommonRewardExecutionResponse> Handle(
        RewardExecutionCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Applying Green rewards for baid {Baid}", request.Baid);
        var saveData = await context.GetOrCreateGreenSaveDataAsync(request.Baid, cancellationToken);
        var activeSeason = gameDataService.Green().ItemShopCatalog.ActiveSeason;
        if (activeSeason is null)
        {
            return new CommonRewardExecutionResponse { Result = 0 };
        }

        var requested = RequestedShopItems(request).Distinct().ToArray();
        var activeCatalogKeys = activeSeason.Items
            .Select(item => (item.ItemType, item.ItemId))
            .ToHashSet();

        if (requested.Any(item => !activeCatalogKeys.Contains(item)))
        {
            logger.LogWarning("Rejecting forged Green shop reward ids for baid {Baid}", request.Baid);
            return new CommonRewardExecutionResponse { Result = 0 };
        }

        var states = await context.GreenShopItemStates
            .Where(row => row.Baid == request.Baid && row.SeasonId == activeSeason.SeasonId)
            .ToDictionaryAsync(row => new ValueTuple<uint, uint>(row.ItemType, row.ItemId), cancellationToken);

        foreach (var item in requested)
        {
            if (!states.TryGetValue(item, out var state)
                || state.Status is not (GreenShopItemStatus.PendingReward or GreenShopItemStatus.Unlocked))
            {
                logger.LogWarning("Rejecting Green shop reward without pending purchase for baid {Baid}", request.Baid);
                return new CommonRewardExecutionResponse { Result = 0 };
            }
        }

        saveData.ToneFlg = GreenShopUnlocks.SetBits(saveData.ToneFlg, request.GetToneNoes, GreenProtocolBytes.ToneFlagBytes);
        saveData.CostumeFlg1 = GreenShopUnlocks.SetBits(saveData.CostumeFlg1, request.GetCostumeNo1s, GreenProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg2 = GreenShopUnlocks.SetBits(saveData.CostumeFlg2, request.GetCostumeNo2s, GreenProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg3 = GreenShopUnlocks.SetBits(saveData.CostumeFlg3, request.GetCostumeNo3s, GreenProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg4 = GreenShopUnlocks.SetBits(saveData.CostumeFlg4, request.GetCostumeNo4s, GreenProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg5 = GreenShopUnlocks.SetBits(saveData.CostumeFlg5, request.GetCostumeNo5s, GreenProtocolBytes.CostumeFlagBytes);

        var now = DateTime.UtcNow;
        foreach (var item in requested)
        {
            var state = states[item];
            state.Status = GreenShopItemStatus.Unlocked;
            state.UnlockedAt ??= now;
        }

        await context.SaveChangesAsync(cancellationToken);
        return new CommonRewardExecutionResponse { Result = 1 };
    }

    private static IEnumerable<(uint ItemType, uint ItemId)> RequestedShopItems(RewardExecutionCommand request)
    {
        foreach (var id in request.ReleaseSongNoes) yield return (1, id);
        foreach (var id in request.GetToneNoes) yield return (2, id);
        foreach (var id in request.GetCostumeNo1s) yield return (3, id);
        foreach (var id in request.GetCostumeNo2s) yield return (5, id);
        foreach (var id in request.GetCostumeNo3s) yield return (4, id);
        foreach (var id in request.GetCostumeNo4s) yield return (6, id);
        foreach (var id in request.GetCostumeNo5s) yield return (7, id);
    }
}
