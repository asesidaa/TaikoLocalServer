using Microsoft.Extensions.Logging.Abstractions;

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

        var policy = profile.Era == GameEra.Green
            ? Ac15NormalStagePolicies.Green
            : Ac15NormalStagePolicies.Standard;

        var playTime = ParsePlayDatetimeOrNow(playResultData.PlayDatetime);
        foreach (var stage in Ac15NormalStageFilter.Filter(baid, playResultData.AryStageInfoes, profile.Limits, policy, NullLogger.Instance))
        {
            var difficulty = MapDifficulty(stage.Level);
            var crown = MapCrown(stage.PlayResult);
            var isShin = stage.StageMode == 1 || stage.StageMode == 4;
            var bestPolicy = policy.GetBestUpdatePolicy(stage, crown);
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
                await UpsertRecentAsync<BlueRecentSongs>(context.BlueRecentSongs, baid, songNo, playTime, cancellationToken);
                return;
            case GameEra.Green:
                await UpsertRecentAsync<GreenRecentSongs>(context.GreenRecentSongs, baid, songNo, playTime, cancellationToken);
                return;
            case GameEra.Yellow:
                await UpsertRecentAsync<YellowRecentSongs>(context.YellowRecentSongs, baid, songNo, playTime, cancellationToken);
                return;
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
                await TrimRecentAsync(context.BlueRecentSongs, context.SaveChangesAsync, baid, maxRecent, cancellationToken);
                return;
            case GameEra.Green:
                await TrimRecentAsync(context.GreenRecentSongs, context.SaveChangesAsync, baid, maxRecent, cancellationToken);
                return;
            case GameEra.Yellow:
                await TrimRecentAsync(context.YellowRecentSongs, context.SaveChangesAsync, baid, maxRecent, cancellationToken);
                return;
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
                AddPlayRow(context.SongPlayDataBlue, row, Ac15NormalPlayMapper.ToBlueSongPlayDatum);
                return;
            case GameEra.Green:
                var play = AddPlayRow(context.SongPlayDataGreen, row, Ac15NormalPlayMapper.ToGreenSongPlayDatum);
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
                AddPlayRow(context.SongPlayDataYellow, row, Ac15NormalPlayMapper.ToYellowSongPlayDatum);
                return;
        }
    }

    private static TPlay AddPlayRow<TPlay>(
        DbSet<TPlay> playRows,
        Ac15PlayRow row,
        Func<Ac15PlayRow, TPlay> create)
        where TPlay : class, IAc15SongPlayDatum
    {
        var play = create(row);
        playRows.Add(play);
        return play;
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
                await UpsertBestAsync(
                    context.SongBestDataBlue,
                    baid,
                    row,
                    policy,
                    Ac15NormalPlayMapper.ToBlueSongBestDatum,
                    cancellationToken);
                return;
            case GameEra.Green:
                await UpsertBestAsync(
                    context.SongBestDataGreen,
                    baid,
                    row,
                    policy,
                    Ac15NormalPlayMapper.ToGreenSongBestDatum,
                    cancellationToken);
                return;
            case GameEra.Yellow:
                await UpsertBestAsync(
                    context.SongBestDataYellow,
                    baid,
                    row,
                    policy,
                    Ac15NormalPlayMapper.ToYellowSongBestDatum,
                    cancellationToken);
                return;
        }
    }

    private static async ValueTask UpsertBestAsync<TBest>(
        DbSet<TBest> bestRows,
        uint baid,
        Ac15BestRow row,
        Ac15BestUpdatePolicy policy,
        Func<uint, Ac15BestRow, bool, TBest> create,
        CancellationToken cancellationToken)
        where TBest : class, IAc15SongBestDatum
    {
        var existing = await bestRows.FindAsync([baid, row.SongId, row.Difficulty, row.IsShin], cancellationToken);
        if (existing is null)
        {
            bestRows.Add(create(baid, row, policy.AllowCrownUpdate));
            return;
        }

        ApplyBestUpdate(existing, row, policy);
    }

    private static void ApplyBestUpdate<TBest>(TBest existing, Ac15BestRow row, Ac15BestUpdatePolicy policy)
        where TBest : IAc15SongBestDatum
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
                await SetFavoriteAsync<BlueFavoriteSongs>(context.BlueFavoriteSongs, baid, songNo, isFavorite, maxFavorites, cancellationToken);
                return;
            case GameEra.Green:
                await SetFavoriteAsync<GreenFavoriteSongs>(context.GreenFavoriteSongs, baid, songNo, isFavorite, maxFavorites, cancellationToken);
                return;
            case GameEra.Yellow:
                await SetFavoriteAsync<YellowFavoriteSongs>(context.YellowFavoriteSongs, baid, songNo, isFavorite, maxFavorites, cancellationToken);
                return;
        }
    }

    private static async ValueTask UpsertRecentAsync<TRecent>(
        DbSet<TRecent> recents,
        uint baid,
        uint songNo,
        DateTime playTime,
        CancellationToken cancellationToken)
        where TRecent : class, IAc15RecentSong, new()
    {
        var recent = await recents.FindAsync([baid, songNo], cancellationToken);
        if (recent is null)
        {
            recents.Add(new TRecent { Baid = baid, SongNo = songNo, LastPlayed = playTime });
            return;
        }

        recent.LastPlayed = playTime;
    }

    private static async ValueTask TrimRecentAsync<TRecent>(
        DbSet<TRecent> recents,
        Func<CancellationToken, Task<int>> saveChanges,
        uint baid,
        int maxRecent,
        CancellationToken cancellationToken)
        where TRecent : class, IAc15RecentSong
    {
        var overage = await recents
            .Where(song => song.Baid == baid)
            .OrderByDescending(song => song.LastPlayed)
            .Skip(maxRecent)
            .ToListAsync(cancellationToken);
        if (overage.Count == 0)
        {
            return;
        }

        recents.RemoveRange(overage);
        await saveChanges(cancellationToken);
    }

    private static async ValueTask SetFavoriteAsync<TFavorite>(
        DbSet<TFavorite> favorites,
        uint baid,
        uint songNo,
        bool isFavorite,
        int maxFavorites,
        CancellationToken cancellationToken)
        where TFavorite : class, IAc15FavoriteSong, new()
    {
        var favorite = await favorites.FindAsync([baid, songNo], cancellationToken);
        if (isFavorite && favorite is null)
        {
            var persisted = await favorites
                .Where(song => song.Baid == baid)
                .Select(song => song.SongNo)
                .ToArrayAsync(cancellationToken);
            var tracked = favorites.Local
                .Where(song => song.Baid == baid)
                .Select(song => song.SongNo);
            if (persisted.Concat(tracked).Distinct().Count() < maxFavorites)
            {
                favorites.Add(new TFavorite { Baid = baid, SongNo = songNo });
            }
        }
        else if (!isFavorite && favorite is not null)
        {
            favorites.Remove(favorite);
        }
    }

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
        => Ac15PlayDatetime.ParseOrNow(playDatetime);

    private static int CrownRank(CrownType crown) => crown switch
    {
        CrownType.Clear => 1,
        CrownType.Gold => 2,
        CrownType.Dondaful => 3,
        _ => 0
    };
}
