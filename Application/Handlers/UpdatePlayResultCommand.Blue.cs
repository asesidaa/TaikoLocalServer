using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class UpdatePlayResultCommandHandler
{
    private const uint BlueDanCostumeId = 36;

    private partial async ValueTask<uint> HandleBlue(
        UpdatePlayResultCommand request,
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

        var playResultData = request.PlayResultData;
        if (playResultData.IsTokkunPlayResult)
        {
            return await HandleBlueTokkun(request.Baid, playResultData, cancellationToken);
        }

        if (playResultData.IsBattlePlayResult)
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
                playTime,
                ApplyCostume))
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
                    saveData.Costume1 = update.DanCostumeId;
                    saveData.CostumeFlg1 = Ac15ProtocolBytes.SetBits(
                        saveData.CostumeFlg1,
                        [update.DanCostumeId],
                        BlueProtocolBytes.CostumeFlagBytes);
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

    private static void ApplyCostume(UserSaveDataBlue saveData, CommonPlayResultData.CostumeData costume)
    {
        saveData.Costume1 = costume.Costume1;
        saveData.Costume2 = costume.Costume2;
        saveData.Costume3 = costume.Costume3;
        saveData.Costume4 = costume.Costume4;
        saveData.Costume5 = costume.Costume5;
        saveData.CostumeFlg1 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg1, [costume.Costume1], BlueProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg2 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg2, [costume.Costume2], BlueProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg3 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg3, [costume.Costume3], BlueProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg4 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg4, [costume.Costume4], BlueProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg5 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg5, [costume.Costume5], BlueProtocolBytes.CostumeFlagBytes);
    }

    private static bool CanAddBlue(uint current, uint delta)
        => CanAddAc15(current, delta);

    private static DateTime ParseBluePlayDatetimeOrNow(string playDatetime)
        => ParseAc15PlayDatetimeOrNow(playDatetime);

}
