using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class UpdatePlayResultCommandHandler
{
    private async ValueTask<uint> HandleBlueBattle(
        uint baid,
        Ac15BlueBattlePlayResult battle,
        Ac15PlayResultMetadata metadata,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var playTime = ParseAc15PlayDatetimeOrNow(metadata.PlayDatetime);

        await context.AddBlueBattleStageResultsAsync(
            baid,
            battle.Stages,
            metadata.PlayMode,
            playTime,
            now,
            cancellationToken);
        await context.ApplyBlueBattleReleaseDataAsync(
            baid,
            battle.ReleaseData,
            now,
            cancellationToken);
        await AddBlueBattleShopDonmedalsAsync(baid, battle.GetDonmedal, now, cancellationToken);
        await UpsertBlueBattleRecentSongsAsync(
            baid,
            battle.Stages,
            playTime,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        await Ac15NormalPlayWriter.TrimRecentAsync(
            context.BlueRecentSongs,
            context.SaveChangesAsync,
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
        IReadOnlyList<Ac15StageResult> stages,
        DateTime playTime,
        CancellationToken cancellationToken)
    {
        foreach (var stage in stages)
        {
            await Ac15NormalPlayWriter.UpsertRecentAsync(
                context.BlueRecentSongs,
                baid,
                stage.SongNo,
                playTime,
                cancellationToken);
        }
    }
}
