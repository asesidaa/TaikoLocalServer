using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class UpdatePlayResultCommandHandler
{
    private const uint BlueDanCostumeId = 36;

    private partial async ValueTask<uint> HandleBlue(
        UpdateAc15PlayResultCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Baid == 0)
        {
            return 1;
        }

        var user = await context.UserData.FindAsync([request.Baid], cancellationToken);
        if (user is null)
        {
            logger.LogWarning("Game uploading a non existing Blue user with baid {Baid}", request.Baid);
            return 1;
        }

        var playResultData = Ac15PlayResultCommonBridge.ToCommon(request.PlayResultData);
        if (IsBlueTokkunShaped(playResultData))
        {
            return await HandleBlueTokkun(request.Baid, playResultData, cancellationToken);
        }

        if (IsBlueBattleShaped(playResultData))
        {
            return await HandleBlueBattle(request.Baid, playResultData, cancellationToken);
        }

        var saveData = await context.GetOrCreateBlueSaveDataAsync(request.Baid, cancellationToken);
        var blue = gameDataService.Blue();
        var shopSeasonState = await context.GetOrCreateActiveBlueShopSeasonStateAsync(
            saveData,
            blue.ItemShopCatalog,
            cancellationToken);

        var validStages = Ac15NormalStageFilter.Filter(
            request.Baid,
            playResultData.AryStageInfoes,
            Ac15EraProfiles.Blue.Limits,
            Ac15NormalStagePolicies.Standard,
            logger);
        if (validStages.Count == 0)
        {
            logger.LogWarning("Skipping Blue playresult with no valid normal stages for baid {Baid}", request.Baid);
            return 1;
        }

        playResultData.AryStageInfoes = validStages.ToList();
        var playTime = ParseAc15PlayDatetimeOrNow(playResultData.PlayDatetime);
        if (!Ac15CommonProfileMutation.TryApply(
                saveData,
                shopSeasonState,
                playResultData,
                validStages,
                Ac15ProfileCounterUpdater.Blue,
                Ac15UnlockFlagAccess.Blue,
                Ac15EraProfiles.Blue.Limits,
                playTime))
        {
            logger.LogWarning("Rejecting invalid Blue medal totals for baid {Baid}", request.Baid);
            return 1;
        }

        await Ac15DaniWriter.SaveAsync(
            BlueDaniTables(),
            playResultData,
            Ac15EraProfiles.Blue.Limits,
            blue.TaikojukuFileOrder.Select(row => new Ac15DaniChallenge(row.ChallengeLevel, row.UniqueId)),
            new Ac15DaniSaveState(saveData.Baid, saveData.DispTaikojukuDan, saveData.IsAutoCostumeOn, BlueDanCostumeId),
            update =>
            {
                saveData.GotDanFlg = update.GotDanFlg;
                saveData.GotDanExtraFlg = update.GotDanExtraFlg;
                saveData.GotDanMax = update.GotDanMax;
                saveData.DispTaikojukuDan = update.DisplayDan;
                if (update.ApplyDanCostume)
                {
                    Ac15CustomizationMutation.ApplyDanCostume(saveData, update.DanCostumeId, Ac15EraProfiles.Blue.Limits);
                }
            },
            logger,
            cancellationToken);

        await Ac15NormalPlayWriter.SaveAsync(
            context,
            BlueNormalPlayTables(),
            new Ac15NormalPlayWriteRequest(request.Baid, playResultData.PlayMode, validStages, Ac15EraProfiles.Blue.Limits, playTime),
            Ac15NormalStagePolicies.Standard,
            cancellationToken);
        return 1;
    }

    private Ac15NormalPlayTables<SongPlayDatumBlue, SongBestDatumBlue, BlueFavoriteSongs, BlueRecentSongs> BlueNormalPlayTables()
        => new(
            context.SongPlayDataBlue,
            context.SongBestDataBlue,
            context.BlueFavoriteSongs,
            context.BlueRecentSongs,
            Ac15NormalPlayMapper.ToBlueSongPlayDatum,
            Ac15NormalPlayMapper.ToBlueSongBestDatum);

    private Ac15DaniTables<DanScoreDatumBlue, DanStageScoreDatumBlue> BlueDaniTables()
        => new(
            context.DanScoreDataBlue,
            context.DanScoreDataBlue.Include(score => score.DanStageScoreData),
            score => score.DanStageScoreData,
            Ac15DaniMapper.ToAc15DaniScore,
            Ac15DaniMapper.ToAc15DaniScoreSummary,
            Ac15DaniMapper.ToBlueDanScoreDatum,
            Ac15DaniMapper.ApplyToBlueDanScoreDatum,
            Ac15DaniMapper.ToBlueDanStageScoreDatum,
            Ac15DaniMapper.ApplyToBlueDanStageScoreDatum);

    private static bool CanAddBlue(uint current, uint delta)
        => CanAddAc15(current, delta);

    private static bool IsBlueTokkunShaped(CommonPlayResultData playResultData)
        => playResultData.IsTokkunPlayResult
           || playResultData.PlayMode == (uint)PlayMode.Tokkun
           || playResultData.TokkunTutorialFlg is not null
           || playResultData.TokkunStageData is not null;

    private static bool IsBlueBattleShaped(CommonPlayResultData playResultData)
        => playResultData.IsBattlePlayResult
           || playResultData.BattleReleaseData is not null
           || playResultData.AryStageInfoes.Any(stage => stage.BattleStageData is not null);
}
