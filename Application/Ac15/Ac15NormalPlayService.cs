using System.Globalization;

namespace TaikoLocalServer.Application.Ac15;

public static class Ac15NormalPlayService
{
    public static async ValueTask<uint> SaveAsync(
        ITaikoDbContext context,
        uint baid,
        CommonPlayResultData playResultData,
        Ac15EraProfile profile,
        IAc15EraHooks hooks,
        CancellationToken cancellationToken)
    {
        if (baid == 0)
        {
            return 1;
        }

        if (await context.UserData.FindAsync([baid], cancellationToken) is null)
        {
            return 1;
        }

        var special = await hooks.TryHandleSpecialPlayModeAsync(
            playResultData,
            new Ac15SpecialModeContext(baid, profile.Era),
            cancellationToken);
        if (special.Action == Ac15SpecialModeAction.Handled)
        {
            return special.Result;
        }

        await GetOrCreateSaveAsync(context, profile, baid, cancellationToken);
        await hooks.BeforeNormalSaveAsync(new Ac15NormalSaveContext(baid, profile.Era, playResultData), cancellationToken);

        var playTime = ParsePlayDatetimeOrNow(playResultData.PlayDatetime);
        foreach (var stage in playResultData.AryStageInfoes)
        {
            if (!IsValidStage(stage, profile))
            {
                continue;
            }

            var hookDecision = hooks.IsSupportedStage(stage);
            if (!hookDecision.IsSupported)
            {
                continue;
            }

            var difficulty = MapDifficulty(stage.Level);
            var crown = MapCrown(stage.PlayResult);
            var isShin = stage.StageMode == 1 || stage.StageMode == 4;
            var bestPolicy = hooks.GetBestUpdatePolicy(stage, crown);
            var row = ToPlayRow(baid, playResultData.PlayMode, stage, difficulty, crown, isShin, playTime);
            AddPlayRow(context, profile, row, cancellationToken);

            if (playResultData.PlayMode != (uint)PlayMode.DanMode || isShin)
            {
                await UpsertBestAsync(
                    context,
                    profile,
                    baid,
                    new Ac15BestRow(stage.SongNo, difficulty, isShin, stage.PlayScore, stage.ScoreRate, crown),
                    bestPolicy,
                    cancellationToken);
            }

            await SetFavoriteAsync(context, profile, baid, stage.SongNo, stage.IsFavorite, profile.Limits.MaxFavoriteSongs, cancellationToken);
            await UpsertRecentAsync(context, profile, baid, stage.SongNo, playTime, cancellationToken);
        }

        await hooks.AfterNormalSaveAsync(new Ac15NormalSaveContext(baid, profile.Era, playResultData), cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        await TrimRecentAsync(context, profile, baid, profile.Limits.MaxRecentSongs, cancellationToken);
        return 1;
    }

    public static async ValueTask UpsertRecentAsync(
        ITaikoDbContext context,
        Ac15EraProfile profile,
        uint baid,
        uint songNo,
        DateTime playTime,
        CancellationToken cancellationToken)
    {
        switch (profile.Era)
        {
            case GameEra.Blue:
            {
                var recent = await context.BlueRecentSongs.FindAsync([baid, songNo], cancellationToken);
                if (recent is null)
                {
                    context.BlueRecentSongs.Add(new BlueRecentSongs { Baid = baid, SongNo = songNo, LastPlayed = playTime });
                }
                else
                {
                    recent.LastPlayed = playTime;
                }

                return;
            }
            case GameEra.Green:
            {
                var recent = await context.GreenRecentSongs.FindAsync([baid, songNo], cancellationToken);
                if (recent is null)
                {
                    context.GreenRecentSongs.Add(new GreenRecentSongs { Baid = baid, SongNo = songNo, LastPlayed = playTime });
                }
                else
                {
                    recent.LastPlayed = playTime;
                }

                return;
            }
            case GameEra.Yellow:
            {
                var recent = await context.YellowRecentSongs.FindAsync([baid, songNo], cancellationToken);
                if (recent is null)
                {
                    context.YellowRecentSongs.Add(new YellowRecentSongs { Baid = baid, SongNo = songNo, LastPlayed = playTime });
                }
                else
                {
                    recent.LastPlayed = playTime;
                }

                return;
            }
        }
    }

    public static async ValueTask TrimRecentAsync(
        ITaikoDbContext context,
        Ac15EraProfile profile,
        uint baid,
        int maxRecent,
        CancellationToken cancellationToken)
    {
        switch (profile.Era)
        {
            case GameEra.Blue:
            {
                var overage = await context.BlueRecentSongs
                    .Where(song => song.Baid == baid)
                    .OrderByDescending(song => song.LastPlayed)
                    .Skip(maxRecent)
                    .ToListAsync(cancellationToken);
                if (overage.Count == 0)
                {
                    return;
                }

                context.BlueRecentSongs.RemoveRange(overage);
                await context.SaveChangesAsync(cancellationToken);
                return;
            }
            case GameEra.Green:
            {
                var overage = await context.GreenRecentSongs
                    .Where(song => song.Baid == baid)
                    .OrderByDescending(song => song.LastPlayed)
                    .Skip(maxRecent)
                    .ToListAsync(cancellationToken);
                if (overage.Count == 0)
                {
                    return;
                }

                context.GreenRecentSongs.RemoveRange(overage);
                await context.SaveChangesAsync(cancellationToken);
                return;
            }
            case GameEra.Yellow:
            {
                var overage = await context.YellowRecentSongs
                    .Where(song => song.Baid == baid)
                    .OrderByDescending(song => song.LastPlayed)
                    .Skip(maxRecent)
                    .ToListAsync(cancellationToken);
                if (overage.Count == 0)
                {
                    return;
                }

                context.YellowRecentSongs.RemoveRange(overage);
                await context.SaveChangesAsync(cancellationToken);
                return;
            }
        }
    }

    private static async ValueTask GetOrCreateSaveAsync(
        ITaikoDbContext context,
        Ac15EraProfile profile,
        uint baid,
        CancellationToken cancellationToken)
    {
        switch (profile.Era)
        {
            case GameEra.Blue:
                _ = await context.GetOrCreateBlueSaveDataAsync(baid, cancellationToken);
                return;
            case GameEra.Green:
                _ = await context.GetOrCreateGreenSaveDataAsync(baid, cancellationToken);
                return;
            case GameEra.Yellow:
                _ = await context.GetOrCreateYellowSaveDataAsync(baid, cancellationToken);
                return;
        }
    }

    private static void AddPlayRow(
        ITaikoDbContext context,
        Ac15EraProfile profile,
        Ac15PlayRow row,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        switch (profile.Era)
        {
            case GameEra.Blue:
                context.SongPlayDataBlue.Add(Ac15NormalPlayMapper.ToBlueSongPlayDatum(row));
                return;
            case GameEra.Green:
                var play = Ac15NormalPlayMapper.ToGreenSongPlayDatum(row);
                context.SongPlayDataGreen.Add(play);
                if (row.GhostStageData is not null)
                {
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

                return;
            case GameEra.Yellow:
                context.SongPlayDataYellow.Add(Ac15NormalPlayMapper.ToYellowSongPlayDatum(row));
                return;
        }
    }

    private static async ValueTask UpsertBestAsync(
        ITaikoDbContext context,
        Ac15EraProfile profile,
        uint baid,
        Ac15BestRow row,
        Ac15BestUpdatePolicy policy,
        CancellationToken cancellationToken)
    {
        switch (profile.Era)
        {
            case GameEra.Blue:
            {
                var existing = await context.SongBestDataBlue.FindAsync([baid, row.SongId, row.Difficulty, row.IsShin], cancellationToken);
                if (existing is null)
                {
                    context.SongBestDataBlue.Add(Ac15NormalPlayMapper.ToBlueSongBestDatum(baid, row, policy.AllowCrownUpdate));
                    return;
                }

                ApplyBestUpdate(existing, row, policy);
                return;
            }
            case GameEra.Green:
            {
                var existing = await context.SongBestDataGreen.FindAsync([baid, row.SongId, row.Difficulty, row.IsShin], cancellationToken);
                if (existing is null)
                {
                    context.SongBestDataGreen.Add(Ac15NormalPlayMapper.ToGreenSongBestDatum(baid, row, policy.AllowCrownUpdate));
                    return;
                }

                ApplyBestUpdate(existing, row, policy);
                return;
            }
            case GameEra.Yellow:
            {
                var existing = await context.SongBestDataYellow.FindAsync([baid, row.SongId, row.Difficulty, row.IsShin], cancellationToken);
                if (existing is null)
                {
                    context.SongBestDataYellow.Add(Ac15NormalPlayMapper.ToYellowSongBestDatum(baid, row, policy.AllowCrownUpdate));
                    return;
                }

                ApplyBestUpdate(existing, row, policy);
                return;
            }
        }
    }

    private static void ApplyBestUpdate(SongBestDatumBlue existing, Ac15BestRow row, Ac15BestUpdatePolicy policy)
    {
        if (policy.AllowScoreUpdate && row.BestScore > existing.BestScore)
        {
            existing.BestScore = row.BestScore;
            existing.BestRate = row.BestRate;
        }

        if (policy.AllowCrownUpdate && CrownRank(row.BestCrown) > CrownRank(existing.BestCrown))
        {
            existing.BestCrown = row.BestCrown;
        }
    }

    private static void ApplyBestUpdate(SongBestDatumGreen existing, Ac15BestRow row, Ac15BestUpdatePolicy policy)
    {
        if (policy.AllowScoreUpdate && row.BestScore > existing.BestScore)
        {
            existing.BestScore = row.BestScore;
            existing.BestRate = row.BestRate;
        }

        if (policy.AllowCrownUpdate && CrownRank(row.BestCrown) > CrownRank(existing.BestCrown))
        {
            existing.BestCrown = row.BestCrown;
        }
    }

    private static void ApplyBestUpdate(SongBestDatumYellow existing, Ac15BestRow row, Ac15BestUpdatePolicy policy)
    {
        if (policy.AllowScoreUpdate && row.BestScore > existing.BestScore)
        {
            existing.BestScore = row.BestScore;
            existing.BestRate = row.BestRate;
        }

        if (policy.AllowCrownUpdate && CrownRank(row.BestCrown) > CrownRank(existing.BestCrown))
        {
            existing.BestCrown = row.BestCrown;
        }
    }

    private static async ValueTask SetFavoriteAsync(
        ITaikoDbContext context,
        Ac15EraProfile profile,
        uint baid,
        uint songNo,
        bool isFavorite,
        int maxFavorites,
        CancellationToken cancellationToken)
    {
        switch (profile.Era)
        {
            case GameEra.Blue:
                await SetBlueFavoriteAsync(context, baid, songNo, isFavorite, maxFavorites, cancellationToken);
                return;
            case GameEra.Green:
                await SetGreenFavoriteAsync(context, baid, songNo, isFavorite, maxFavorites, cancellationToken);
                return;
            case GameEra.Yellow:
                await SetYellowFavoriteAsync(context, baid, songNo, isFavorite, maxFavorites, cancellationToken);
                return;
        }
    }

    private static async ValueTask SetBlueFavoriteAsync(
        ITaikoDbContext context,
        uint baid,
        uint songNo,
        bool isFavorite,
        int maxFavorites,
        CancellationToken cancellationToken)
    {
        var favorite = await context.BlueFavoriteSongs.FindAsync([baid, songNo], cancellationToken);
        if (isFavorite && favorite is null)
        {
            var persisted = await context.BlueFavoriteSongs
                .Where(song => song.Baid == baid)
                .Select(song => song.SongNo)
                .ToArrayAsync(cancellationToken);
            var tracked = context.BlueFavoriteSongs.Local
                .Where(song => song.Baid == baid)
                .Select(song => song.SongNo);
            if (persisted.Concat(tracked).Distinct().Count() < maxFavorites)
            {
                context.BlueFavoriteSongs.Add(new BlueFavoriteSongs { Baid = baid, SongNo = songNo });
            }
        }
        else if (!isFavorite && favorite is not null)
        {
            context.BlueFavoriteSongs.Remove(favorite);
        }
    }

    private static async ValueTask SetGreenFavoriteAsync(
        ITaikoDbContext context,
        uint baid,
        uint songNo,
        bool isFavorite,
        int maxFavorites,
        CancellationToken cancellationToken)
    {
        var favorite = await context.GreenFavoriteSongs.FindAsync([baid, songNo], cancellationToken);
        if (isFavorite && favorite is null)
        {
            var count = await context.GreenFavoriteSongs.CountAsync(song => song.Baid == baid, cancellationToken);
            if (count < maxFavorites)
            {
                context.GreenFavoriteSongs.Add(new GreenFavoriteSongs { Baid = baid, SongNo = songNo });
            }
        }
        else if (!isFavorite && favorite is not null)
        {
            context.GreenFavoriteSongs.Remove(favorite);
        }
    }

    private static async ValueTask SetYellowFavoriteAsync(
        ITaikoDbContext context,
        uint baid,
        uint songNo,
        bool isFavorite,
        int maxFavorites,
        CancellationToken cancellationToken)
    {
        var favorite = await context.YellowFavoriteSongs.FindAsync([baid, songNo], cancellationToken);
        if (isFavorite && favorite is null)
        {
            var persisted = await context.YellowFavoriteSongs
                .Where(song => song.Baid == baid)
                .Select(song => song.SongNo)
                .ToArrayAsync(cancellationToken);
            var tracked = context.YellowFavoriteSongs.Local
                .Where(song => song.Baid == baid)
                .Select(song => song.SongNo);
            if (persisted.Concat(tracked).Distinct().Count() < maxFavorites)
            {
                context.YellowFavoriteSongs.Add(new YellowFavoriteSongs { Baid = baid, SongNo = songNo });
            }
        }
        else if (!isFavorite && favorite is not null)
        {
            context.YellowFavoriteSongs.Remove(favorite);
        }
    }

    private static bool IsValidStage(CommonPlayResultData.StageData stage, Ac15EraProfile profile)
        => stage.SongNo < profile.Limits.SongFlagBytes * 8
           && stage.Level >= profile.Limits.MinCourseLevel
           && stage.Level <= profile.Limits.MaxCourseLevel;

    private static Difficulty MapDifficulty(uint level) => level switch
    {
        1 => Difficulty.Easy,
        2 => Difficulty.Normal,
        3 => Difficulty.Hard,
        4 => Difficulty.Oni,
        5 => Difficulty.UraOni,
        _ => Difficulty.None
    };

    private static CrownType MapCrown(uint playResult) => playResult switch
    {
        1 => CrownType.Clear,
        2 => CrownType.Gold,
        3 => CrownType.Dondaful,
        _ => CrownType.None
    };

    private static Ac15PlayRow ToPlayRow(
        uint baid,
        uint playMode,
        CommonPlayResultData.StageData stage,
        Difficulty difficulty,
        CrownType crown,
        bool isShin,
        DateTime playTime)
        => new(
            baid,
            stage.SongNo,
            difficulty,
            crown,
            stage.PlayScore,
            stage.ScoreRate,
            stage.GoodCnt,
            stage.OkCnt,
            stage.NgCnt,
            stage.ComboCnt,
            stage.HitCnt,
            stage.PoundCnt,
            stage.StarLevel,
            stage.SupportLevel,
            stage.OptionFlg,
            stage.ToneFlg,
            playMode,
            stage.StageMode,
            isShin,
            stage.MusicCateg,
            stage.SelectedFolderId,
            stage.IsFavorite,
            stage.IsRecent,
            stage.IsPapamama,
            stage.IsPushed,
            stage.SoulGauge.GetValueOrDefault(),
            stage.PlayDan.GetValueOrDefault(),
            stage.WaiwaiResult.GetValueOrDefault(),
            stage.WaiwaiGauge.GetValueOrDefault(),
            stage.GhostStageData,
            playTime);

    private static DateTime ParsePlayDatetimeOrNow(string playDatetime)
    {
        var formats = new[] { Constants.DateTimeFormat, "yyyy-MM-dd HH:mm:ss" };
        return DateTime.TryParseExact(
            playDatetime,
            formats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var parsed)
            ? parsed
            : DateTime.Now;
    }

    private static int CrownRank(CrownType crown) => crown switch
    {
        CrownType.Clear => 1,
        CrownType.Gold => 2,
        CrownType.Dondaful => 3,
        _ => 0
    };
}
