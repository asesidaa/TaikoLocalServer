using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class UpdatePlayResultCommandHandler
{
    private const uint GreenDanCostumeId = 36;

    private partial async ValueTask<uint> HandleGreen(
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
            logger.LogWarning("Game uploading a non existing Green user with baid {Baid}", request.Baid);
            return 1;
        }

        var playResultData = request.PlayResultData;
        var saveData = await context.GetOrCreateGreenSaveDataAsync(request.Baid, cancellationToken);
        var green = gameDataService.Green();
        var activeShopSeason = green.ItemShopCatalog.ActiveSeason;
        var shopSeasonState = activeShopSeason is null
            ? null
            : await context.GetOrCreateGreenShopSeasonStateAsync(saveData, activeShopSeason.SeasonId, cancellationToken);

        var currentDonmedal = shopSeasonState?.TotalGetDonmedal ?? saveData.TotalGetDonmedal;
        if (HasInvalidAc15MedalTotals(currentDonmedal, saveData.TotalGetKatsumedal, playResultData))
        {
            logger.LogWarning("Rejecting invalid Green medal totals for baid {Baid}", request.Baid);
            return 1;
        }

        var validStages = Ac15NormalStageFilter.Filter(
            request.Baid,
            playResultData.AryStageInfoes,
            Ac15EraProfiles.Green.Limits,
            Ac15NormalStagePolicies.Green,
            logger);
        if (validStages.Count == 0)
        {
            logger.LogWarning("Skipping Green playresult with no valid normal stages for baid {Baid}", request.Baid);
            return 1;
        }

        playResultData.AryStageInfoes = validStages.ToList();

        var playTime = ParseAc15PlayDatetimeOrNow(playResultData.PlayDatetime);

        AddAc15Donmedals(shopSeasonState, delta => saveData.TotalGetDonmedal += delta, playResultData.GetDonmedal);

        saveData.TotalGetKatsumedal += playResultData.GetKatsumedal;
        saveData.ItemshopTutorialFlg = playResultData.ItemshopTutorialFlg ?? saveData.ItemshopTutorialFlg;
        saveData.IsDevil = playResultData.IsDevil ?? saveData.IsDevil;
        saveData.IsExplain = playResultData.IsExplain ?? saveData.IsExplain;
        saveData.WaiwaiTutorialFlg = playResultData.WaiwaiTutorialFlg ?? saveData.WaiwaiTutorialFlg;
        if (playResultData.HasDifficultyPlayedCourse)
        {
            saveData.DifficultyPlayedCourse = playResultData.DifficultyPlayedCourse;
        }

        if (playResultData.HasDifficultyPlayedStar)
        {
            saveData.DifficultyPlayedStar = playResultData.DifficultyPlayedStar;
        }

        saveData.LastPlayDatetime = playTime;
        saveData.PrevAreaCode = playResultData.AreaCode;

        if (playResultData.HasAryCurrentCostume && saveData.IsAutoCostumeOn)
        {
            ApplyCostume(saveData, playResultData.AryCurrentCostume);
        }

        ApplyUnlockBits(saveData, playResultData);
        await ApplyGhostUpdatesAsync(saveData, playResultData, cancellationToken);

        foreach (var stage in playResultData.AryStageInfoes)
        {
            Ac15ProfileCounterUpdater.ApplyGreenStage(saveData, stage);
        }

        ApplyGhostPlayedSongBits(saveData, playResultData);

        await Ac15DaniService.SaveAsync(
            context,
            playResultData,
            Ac15EraProfiles.Green,
            green.TaikojukuFileOrder.Select(row => new Ac15DaniChallenge(row.ChallengeLevel, row.UniqueId)),
            new Ac15DaniSaveState(saveData.Baid, saveData.DispTaikojukuDan, saveData.IsAutoCostumeOn, GreenDanCostumeId),
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
                        GreenProtocolBytes.CostumeFlagBytes);
                }
            },
            logger,
            cancellationToken);

        return await Ac15NormalPlayService.SaveAsync(
            context,
            request.Baid,
            playResultData,
            Ac15EraProfiles.Green,
            new GreenAc15NormalPlayHooks(),
            cancellationToken);
    }

    private static void ApplyCostume(UserSaveDataGreen saveData, CommonPlayResultData.CostumeData costume)
    {
        saveData.Costume1 = costume.Costume1;
        saveData.Costume2 = costume.Costume2;
        saveData.Costume3 = costume.Costume3;
        saveData.Costume4 = costume.Costume4;
        saveData.Costume5 = costume.Costume5;
        saveData.CostumeFlg1 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg1, [costume.Costume1], GreenProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg2 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg2, [costume.Costume2], GreenProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg3 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg3, [costume.Costume3], GreenProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg4 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg4, [costume.Costume4], GreenProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg5 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg5, [costume.Costume5], GreenProtocolBytes.CostumeFlagBytes);
    }

    private static void ApplyUnlockBits(UserSaveDataGreen saveData, CommonPlayResultData playResultData)
    {
        saveData.ToneFlg = Ac15ProtocolBytes.SetBits(saveData.ToneFlg, playResultData.GetToneNoes, GreenProtocolBytes.ToneFlagBytes);
        saveData.CostumeFlg1 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg1, playResultData.GetCostumeNo1s, GreenProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg2 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg2, playResultData.GetCostumeNo2s, GreenProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg3 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg3, playResultData.GetCostumeNo3s, GreenProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg4 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg4, playResultData.GetCostumeNo4s, GreenProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg5 = Ac15ProtocolBytes.SetBits(saveData.CostumeFlg5, playResultData.GetCostumeNo5s, GreenProtocolBytes.CostumeFlagBytes);
        saveData.TitleFlg = Ac15ProtocolBytes.SetBits(saveData.TitleFlg, playResultData.GetTitleNoes, GreenProtocolBytes.TitleFlagBytes);
    }

    private static void ApplyGhostPlayedSongBits(UserSaveDataGreen saveData, CommonPlayResultData playResultData)
    {
        var aiBattleSongNos = playResultData.AryStageInfoes
            .Where(stage => GreenStageModeInterpreter.IsAiBattle(stage.StageMode))
            .Select(stage => stage.SongNo);

        saveData.GhostPlayedSongFlag = Ac15ProtocolBytes.SetBits(
            saveData.GhostPlayedSongFlag,
            aiBattleSongNos,
            GreenProtocolBytes.GhostPlayedSongBytes);
    }

    private async Task ApplyGhostUpdatesAsync(UserSaveDataGreen saveData, CommonPlayResultData playResultData, CancellationToken cancellationToken)
    {
        if (playResultData.GhostReleaseData is not null)
        {
            saveData.GhostReleaseInfoFlag = Ac15ProtocolBytes.SetBits(
                saveData.GhostReleaseInfoFlag,
                playResultData.GhostReleaseData.ReleaseInfoId,
                GreenProtocolBytes.GhostReleaseInfoBytes);

            foreach (var token in playResultData.GhostReleaseData.AryTokendata)
            {
                var existing = await context.GreenGhostTokens.FindAsync([saveData.Baid, token.TokenId], cancellationToken);
                if (existing is null)
                {
                    context.GreenGhostTokens.Add(new GreenGhostTokens
                    {
                        Baid = saveData.Baid,
                        TokenId = token.TokenId,
                        TokenValue = token.TokenValue
                    });
                }
                else
                {
                    existing.TokenValue = token.TokenValue;
                }
            }
        }

        if (playResultData.GhostUpdatePerfData is not null)
        {
            saveData.GhostInputMedian = playResultData.GhostUpdatePerfData.InputMedian;
            saveData.GhostInputVariance = playResultData.GhostUpdatePerfData.InputVariance;
        }

        if (playResultData.GhostUpdateRankData is null)
        {
            return;
        }

        saveData.GhostRankId = playResultData.GhostUpdateRankData.RankId;
        saveData.GhostWinPoint = playResultData.GhostUpdateRankData.WinPoint;
        saveData.GhostCertifiedLevelId = playResultData.GhostUpdateRankData.CertifiedLevelId;
        saveData.GhostTotalWinnings = (uint)Math.Min(
            uint.MaxValue,
            playResultData.GhostUpdateRankData.AryWinningsData.Sum(row => (long)row.Winnings));

        foreach (var winning in playResultData.GhostUpdateRankData.AryWinningsData)
        {
            var existing = await context.GreenGhostWinnings.FindAsync([saveData.Baid, winning.LevelId], cancellationToken);
            if (existing is null)
            {
                context.GreenGhostWinnings.Add(new GreenGhostWinnings
                {
                    Baid = saveData.Baid,
                    LevelId = winning.LevelId,
                    Winnings = winning.Winnings
                });
            }
            else
            {
                existing.Winnings = winning.Winnings;
            }
        }
    }

}
