using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15NormalPlayTables<TPlay, TBest, TFavorite, TRecent>(
    DbSet<TPlay> PlayRows,
    DbSet<TBest> BestRows,
    DbSet<TFavorite> FavoriteRows,
    DbSet<TRecent> RecentRows,
    Func<Ac15PlayRow, TPlay> CreatePlay,
    Func<uint, Ac15BestRow, bool, TBest> CreateBest,
    Action<TPlay, Ac15PlayRow>? AfterAddPlayRow = null)
    where TPlay : class, IAc15SongPlayDatum
    where TBest : class, IAc15SongBestDatum
    where TFavorite : class, IAc15FavoriteSong, new()
    where TRecent : class, IAc15RecentSong, new();

public sealed record Ac15NormalPlayWriteRequest(
    uint Baid,
    uint PlayMode,
    IReadOnlyList<Ac15StageResult> Stages,
    Ac15ProtocolLimits Limits,
    DateTime PlayTime);

public static class Ac15NormalPlayWriter
{
    public static async ValueTask SaveAsync<TPlay, TBest, TFavorite, TRecent>(
        ITaikoDbContext context,
        Ac15NormalPlayTables<TPlay, TBest, TFavorite, TRecent> tables,
        Ac15NormalPlayWriteRequest request,
        Ac15NormalStagePolicy policy,
        CancellationToken cancellationToken)
        where TPlay : class, IAc15SongPlayDatum
        where TBest : class, IAc15SongBestDatum
        where TFavorite : class, IAc15FavoriteSong, new()
        where TRecent : class, IAc15RecentSong, new()
    {
        foreach (var stage in request.Stages)
        {
            var difficulty = MapDifficulty(stage.Level);
            var crown = MapCrown(stage.PlayResult);
            var isShin = stage.StageMode == 1 || stage.StageMode == 4;
            var bestPolicy = policy.GetBestUpdatePolicy(stage, crown);
            var playRow = ToPlayRow(request.Baid, request.PlayMode, stage, difficulty, crown, isShin, request.PlayTime);
            var play = tables.CreatePlay(playRow);
            tables.PlayRows.Add(play);
            tables.AfterAddPlayRow?.Invoke(play, playRow);

            if (request.PlayMode != (uint)PlayMode.DanMode || isShin)
            {
                await UpsertBestAsync(
                    tables.BestRows,
                    request.Baid,
                    new Ac15BestRow(stage.SongNo, difficulty, isShin, stage.PlayScore, stage.ScoreRate, crown),
                    bestPolicy,
                    tables.CreateBest,
                    cancellationToken);
            }

            await SetFavoriteAsync(tables.FavoriteRows, request.Baid, stage.SongNo, stage.IsFavorite, request.Limits.MaxFavoriteSongs, cancellationToken);
            await UpsertRecentAsync(tables.RecentRows, request.Baid, stage.SongNo, request.PlayTime, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
        await TrimRecentAsync(tables.RecentRows, context.SaveChangesAsync, request.Baid, request.Limits.MaxRecentSongs, cancellationToken);
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
            var persisted = await favorites.Where(song => song.Baid == baid).Select(song => song.SongNo).ToArrayAsync(cancellationToken);
            var tracked = favorites.Local.Where(song => song.Baid == baid).Select(song => song.SongNo);
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

    public static async ValueTask UpsertRecentAsync<TRecent>(
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

    public static async ValueTask TrimRecentAsync<TRecent>(
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
        Ac15StageResult stage,
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
            stage.GreenGhostStage,
            playTime);

    private static int CrownRank(CrownType crown) => crown switch
    {
        CrownType.Clear => 1,
        CrownType.Gold => 2,
        CrownType.Dondaful => 3,
        _ => 0
    };
}
