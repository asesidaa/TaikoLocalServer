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
        if (!Ac15CommonProfileMutation.TryApply(
                saveData,
                shopSeasonState,
                playResultData,
                validStages,
                Ac15ProfileCounterUpdater.Green,
                Ac15UnlockFlagAccess.Green,
                Ac15EraProfiles.Green.Limits,
                playTime,
                ApplyCostume))
        {
            logger.LogWarning("Rejecting invalid Green medal totals for baid {Baid}", request.Baid);
            return 1;
        }

        await ApplyGhostUpdatesAsync(saveData, playResultData, cancellationToken);
        ApplyGhostPlayedSongBits(saveData, playResultData);

        await Ac15DaniWriter.SaveAsync(
            GreenDaniTables(),
            playResultData,
            Ac15EraProfiles.Green.Limits,
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

        await Ac15NormalPlayWriter.SaveAsync(
            context,
            GreenNormalPlayTables(),
            new Ac15NormalPlayWriteRequest(request.Baid, playResultData.PlayMode, validStages, Ac15EraProfiles.Green.Limits, playTime),
            Ac15NormalStagePolicies.Green,
            cancellationToken);
        return 1;
    }

    private Ac15NormalPlayTables<SongPlayDatumGreen, SongBestDatumGreen, GreenFavoriteSongs, GreenRecentSongs> GreenNormalPlayTables()
        => new(
            context.SongPlayDataGreen,
            context.SongBestDataGreen,
            context.GreenFavoriteSongs,
            context.GreenRecentSongs,
            Ac15NormalPlayMapper.ToGreenSongPlayDatum,
            Ac15NormalPlayMapper.ToGreenSongBestDatum,
            AfterAddPlayRow: AddGreenGhostStageSections);

    private Ac15DaniTables<DanScoreDatumGreen, DanStageScoreDatumGreen> GreenDaniTables()
        => new(
            context.DanScoreDataGreen,
            context.DanScoreDataGreen.Include(score => score.DanStageScoreData),
            score => score.DanStageScoreData,
            Ac15DaniMapper.ToAc15DaniScore,
            Ac15DaniMapper.ToAc15DaniScoreSummary,
            Ac15DaniMapper.ToGreenDanScoreDatum,
            Ac15DaniMapper.ApplyToGreenDanScoreDatum,
            Ac15DaniMapper.ToGreenDanStageScoreDatum,
            Ac15DaniMapper.ApplyToGreenDanStageScoreDatum);

    private void AddGreenGhostStageSections(SongPlayDatumGreen play, Ac15PlayRow row)
    {
        if (row.GhostStageData is null)
        {
            return;
        }

        uint sectionNo = 0;
        foreach (var section in row.GhostStageData.ArySectionData)
        {
            context.GhostStageSectionDataGreen.Add(new GhostStageSectionDatumGreen
            {
                Parent = play,
                SectionNo = sectionNo++,
                IsWin = section.IsWin,
                GoodCount = section.GoodCnt,
                OkCount = section.OkCnt,
                NgCount = section.NgCnt,
                PoundCount = section.PoundCnt
            });
        }
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
