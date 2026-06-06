namespace TaikoLocalServer.Application.Ac15;

public sealed class BlueAc15NormalPlayAdapter(ITaikoDbContext context) : IAc15NormalPlayPersistence
{
    public async ValueTask<bool> UserExistsAsync(uint baid, CancellationToken cancellationToken)
        => await context.UserData.FindAsync([baid], cancellationToken) is not null;

    public async ValueTask<Ac15SaveSnapshot> GetOrCreateSaveAsync(uint baid, CancellationToken cancellationToken)
    {
        _ = await context.GetOrCreateBlueSaveDataAsync(baid, cancellationToken);
        return new Ac15SaveSnapshot(baid);
    }

    public ValueTask AddPlayRowAsync(Ac15PlayRow row, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        context.SongPlayDataBlue.Add(new SongPlayDatumBlue
        {
            Baid = row.Baid,
            SongId = row.SongId,
            Difficulty = row.Difficulty,
            Crown = row.Crown,
            Score = row.Score,
            ScoreRate = row.ScoreRate,
            GoodCount = row.GoodCount,
            OkCount = row.OkCount,
            MissCount = row.MissCount,
            ComboCount = row.ComboCount,
            HitCount = row.HitCount,
            PoundCount = row.PoundCount,
            StarLevel = row.StarLevel,
            OptionFlg = row.OptionFlg,
            ToneFlg = row.ToneFlg,
            PlayMode = row.PlayMode,
            StageMode = row.StageMode,
            IsShin = row.IsShin,
            MusicCategory = row.MusicCategory,
            SelectedFolderId = row.SelectedFolderId,
            IsFavorite = row.IsFavorite,
            IsRecent = row.IsRecent,
            IsPapamama = row.IsPapamama,
            IsPushed = row.IsPushed,
            SoulGauge = row.SoulGauge,
            PlayDan = row.PlayDan,
            WaiwaiResult = row.WaiwaiResult,
            WaiwaiGauge = row.WaiwaiGauge,
            PlayTime = row.PlayTime
        });
        return ValueTask.CompletedTask;
    }

    public async ValueTask UpsertBestAsync(uint baid, Ac15BestRow row, Ac15BestUpdatePolicy policy, CancellationToken cancellationToken)
    {
        var existing = await context.SongBestDataBlue.FindAsync([baid, row.SongId, row.Difficulty, row.IsShin], cancellationToken);
        if (existing is null)
        {
            context.SongBestDataBlue.Add(new SongBestDatumBlue
            {
                Baid = baid,
                SongId = row.SongId,
                Difficulty = row.Difficulty,
                IsShin = row.IsShin,
                BestScore = row.BestScore,
                BestRate = row.BestRate,
                BestCrown = policy.AllowCrownUpdate ? row.BestCrown : CrownType.None
            });
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

    public async ValueTask SetFavoriteAsync(uint baid, uint songNo, bool isFavorite, int maxFavorites, CancellationToken cancellationToken)
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

    public async ValueTask UpsertRecentAsync(uint baid, uint songNo, DateTime playTime, CancellationToken cancellationToken)
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
    }

    public async ValueTask TrimRecentAsync(uint baid, int maxRecent, CancellationToken cancellationToken)
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
    }

    public ValueTask SaveChangesAsync(CancellationToken cancellationToken)
        => new(context.SaveChangesAsync(cancellationToken));

    private static int CrownRank(CrownType crown) => crown switch
    {
        CrownType.Clear => 1,
        CrownType.Gold => 2,
        CrownType.Dondaful => 3,
        _ => 0
    };
}
