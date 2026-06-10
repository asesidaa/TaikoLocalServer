using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class UpdatePlayResultCommandHandler
{
    private async ValueTask<uint> HandleBlueBattle(
        uint baid,
        CommonPlayResultData playResultData,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var playTime = ParseBluePlayDatetimeOrNow(playResultData.PlayDatetime);

        await context.AddBlueBattleStageResultsAsync(
            baid,
            playResultData,
            playTime,
            now,
            cancellationToken);
        await context.ApplyBlueBattleReleaseDataAsync(
            baid,
            playResultData.BattleReleaseData,
            now,
            cancellationToken);
        await AddBlueBattleShopDonmedalsAsync(baid, playResultData.GetDonmedal, now, cancellationToken);
        await UpsertBlueBattleRecentSongsAsync(
            baid,
            playResultData,
            playTime,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        await Ac15NormalPlayService.TrimRecentAsync(
            context,
            Ac15EraProfiles.Blue,
            baid,
            Ac15EraProfiles.Blue.Limits.MaxRecentSongs,
            cancellationToken);
        return 1;
    }

    private async Task AddBlueBattleShopDonmedalsAsync(
        uint baid,
        uint getDonmedal,
        DateTime now,
        CancellationToken cancellationToken)
    {
        if (getDonmedal == 0)
        {
            return;
        }

        var shopSeasonState = await context.GetOrCreateActiveBlueShopSeasonStateAsync(
            baid,
            gameDataService.Blue().ItemShopCatalog,
            cancellationToken);
        if (shopSeasonState is null)
        {
            return;
        }

        if (!CanAddBlue(shopSeasonState.TotalGetDonmedal, getDonmedal))
        {
            logger.LogWarning("Skipping invalid Blue battle shop Don medal total for baid {Baid}", baid);
            return;
        }

        shopSeasonState.TotalGetDonmedal += getDonmedal;
        shopSeasonState.UpdatedAt = now;
    }

    private async Task UpsertBlueBattleRecentSongsAsync(
        uint baid,
        CommonPlayResultData playResultData,
        DateTime playTime,
        CancellationToken cancellationToken)
    {
        foreach (var stage in playResultData.AryStageInfoes)
        {
            await Ac15NormalPlayService.UpsertRecentAsync(
                context,
                Ac15EraProfiles.Blue,
                baid,
                stage.SongNo,
                playTime,
                cancellationToken);
        }
    }
}
