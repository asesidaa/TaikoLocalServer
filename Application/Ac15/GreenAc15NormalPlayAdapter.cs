namespace TaikoLocalServer.Application.Ac15;

public sealed class GreenAc15NormalPlayAdapter(ITaikoDbContext context) : IAc15NormalPlayPersistence
{
    public async ValueTask<bool> UserExistsAsync(uint baid, CancellationToken cancellationToken)
        => await context.UserData.FindAsync([baid], cancellationToken) is not null;

    public async ValueTask<Ac15SaveSnapshot> GetOrCreateSaveAsync(uint baid, CancellationToken cancellationToken)
    {
        _ = await context.GetOrCreateGreenSaveDataAsync(baid, cancellationToken);
        return new Ac15SaveSnapshot(baid);
    }

    public ValueTask AddPlayRowAsync(Ac15PlayRow row, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var play = new SongPlayDatumGreen
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
            SupportLevel = row.SupportLevel,
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
        };

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

        return ValueTask.CompletedTask;
    }

    public async ValueTask UpsertBestAsync(uint baid, Ac15BestRow row, Ac15BestUpdatePolicy policy, CancellationToken cancellationToken)
    {
        var existing = await context.SongBestDataGreen.FindAsync([baid, row.SongId, row.Difficulty, row.IsShin], cancellationToken);
        if (existing is null)
        {
            context.SongBestDataGreen.Add(new SongBestDatumGreen
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

    public async ValueTask UpsertRecentAsync(uint baid, uint songNo, DateTime playTime, CancellationToken cancellationToken)
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
    }

    public async ValueTask TrimRecentAsync(uint baid, int maxRecent, CancellationToken cancellationToken)
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

public sealed class GreenAc15NormalPlayHooks : IAc15EraHooks
{
    public ValueTask<Ac15SpecialModeResult> TryHandleSpecialPlayModeAsync(
        CommonPlayResultData request,
        Ac15SpecialModeContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(Ac15SpecialModeResult.ContinueNormal());
    }

    public Ac15StageSupportDecision IsSupportedStage(CommonPlayResultData.StageData stage)
        => stage.StageMode is 0 or 1 or 3 or 4
            ? Ac15StageSupportDecision.Supported
            : Ac15StageSupportDecision.Unsupported($"stage_mode {stage.StageMode} is not supported by Green");

    public Ac15BestUpdatePolicy GetBestUpdatePolicy(CommonPlayResultData.StageData stage, CrownType crown)
    {
        var isAiBattle = GreenStageModeInterpreter.IsAiBattle(stage.StageMode);
        var allowCrownUpdate = !isAiBattle || GreenAiBattleLevels.AllowsCrown(stage.Level, stage.SupportLevel);
        return new Ac15BestUpdatePolicy(AllowScoreUpdate: true, AllowCrownUpdate: allowCrownUpdate);
    }

    public ValueTask BeforeNormalSaveAsync(Ac15NormalSaveContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.CompletedTask;
    }

    public ValueTask AfterNormalSaveAsync(Ac15NormalSaveContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.CompletedTask;
    }

    public void BuildUserDataExtras(CommonUserDataResponse response, Ac15UserDataContext context)
    {
    }

    public void BuildInitialDataExtras(CommonInitialDataCheckResponse response, Ac15InitialDataContext context)
    {
    }
}
