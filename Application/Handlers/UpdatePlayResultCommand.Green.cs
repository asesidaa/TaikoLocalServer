using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class UpdatePlayResultCommandHandler
{
    private const uint GreenDanCostumeId = 36;

    private partial async ValueTask<uint> HandleGreen(
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
            logger.LogWarning("Game uploading a non existing Green user with baid {Baid}", request.Baid);
            return 1;
        }

        var playResultData = request.PlayResultData;
        var normal = playResultData.Normal;
        IReadOnlyList<Ac15StageResult> stages = normal?.Stages ?? [];
        var saveData = await context.GetOrCreateGreenSaveDataAsync(request.Baid, cancellationToken);
        var green = gameDataService.Green();
        var activeShopSeason = green.ItemShopCatalog.ActiveSeason;
        var shopSeasonState = activeShopSeason is null
            ? null
            : await context.GetOrCreateGreenShopSeasonStateAsync(saveData, activeShopSeason.SeasonId, cancellationToken);

        var validStages = Ac15NormalStageFilter.Filter(
            request.Baid,
            stages,
            Ac15EraProfiles.Green.Limits,
            Ac15NormalStagePolicies.Green,
            logger);
        if (validStages.Count == 0)
        {
            logger.LogWarning("Skipping Green playresult with no valid normal stages for baid {Baid}", request.Baid);
            return 1;
        }

        var playTime = ParseAc15PlayDatetimeOrNow(playResultData.Metadata.PlayDatetime);
        if (!Ac15CommonProfileMutation.TryApply(
                saveData,
                shopSeasonState,
                playResultData.Profile,
                validStages,
                Ac15ProfileCounterUpdater.Green,
                Ac15UnlockFlagAccess.Green,
                Ac15EraProfiles.Green.Limits,
                playTime))
        {
            logger.LogWarning("Rejecting invalid Green medal totals for baid {Baid}", request.Baid);
            return 1;
        }

        await ApplyGhostUpdatesAsync(saveData, playResultData.GreenGhost, cancellationToken);
        ApplyGhostPlayedSongBits(saveData, validStages);

        var dani = playResultData.Metadata.PlayMode == (uint)PlayMode.DanMode && playResultData.Dani is { } inputDani
            ? inputDani with { Stages = validStages.ToList() }
            : null;

        await Ac15DaniWriter.SaveAsync(
            GreenDaniTables(),
            dani,
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
                    Ac15CustomizationMutation.ApplyDanCostume(saveData, update.DanCostumeId, Ac15EraProfiles.Green.Limits);
                }
            },
            logger,
            cancellationToken);

        await Ac15NormalPlayWriter.SaveAsync(
            context,
            GreenNormalPlayTables(),
            new Ac15NormalPlayWriteRequest(request.Baid, playResultData.Metadata.PlayMode, validStages, Ac15EraProfiles.Green.Limits, playTime),
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

    private static void ApplyGhostPlayedSongBits(UserSaveDataGreen saveData, IReadOnlyList<Ac15StageResult> stages)
    {
        var aiBattleSongNos = stages
            .Where(stage => GreenStageModeInterpreter.IsAiBattle(stage.StageMode))
            .Select(stage => stage.SongNo);

        saveData.GhostPlayedSongFlag = Ac15ProtocolBytes.SetBits(
            saveData.GhostPlayedSongFlag,
            aiBattleSongNos,
            GreenProtocolBytes.GhostPlayedSongBytes);
    }

    private async Task ApplyGhostUpdatesAsync(
        UserSaveDataGreen saveData,
        Ac15GreenGhostPlayResult? ghost,
        CancellationToken cancellationToken)
    {
        if (ghost is null)
        {
            return;
        }

        if (ghost.ReleaseData is not null)
        {
            saveData.GhostReleaseInfoFlag = Ac15ProtocolBytes.SetBits(
                saveData.GhostReleaseInfoFlag,
                ghost.ReleaseData.ReleaseInfoId,
                GreenProtocolBytes.GhostReleaseInfoBytes);

            foreach (var token in ghost.ReleaseData.AryTokendata)
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

        if (ghost.PerfData is not null)
        {
            saveData.GhostInputMedian = ghost.PerfData.InputMedian;
            saveData.GhostInputVariance = ghost.PerfData.InputVariance;
        }

        if (ghost.RankData is null)
        {
            return;
        }

        saveData.GhostRankId = ghost.RankData.RankId;
        saveData.GhostWinPoint = ghost.RankData.WinPoint;
        saveData.GhostCertifiedLevelId = ghost.RankData.CertifiedLevelId;
        saveData.GhostTotalWinnings = (uint)Math.Min(
            uint.MaxValue,
            ghost.RankData.AryWinningsData.Sum(row => (long)row.Winnings));

        foreach (var winning in ghost.RankData.AryWinningsData)
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
