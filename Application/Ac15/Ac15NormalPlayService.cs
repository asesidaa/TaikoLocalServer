using System.Globalization;

namespace TaikoLocalServer.Application.Ac15;

public static class Ac15NormalPlayService
{
    public static async ValueTask<uint> SaveAsync(
        uint baid,
        CommonPlayResultData playResultData,
        Ac15EraProfile profile,
        IAc15NormalPlayPersistence persistence,
        IAc15EraHooks hooks,
        CancellationToken cancellationToken)
    {
        if (baid == 0)
        {
            return 1;
        }

        if (!await persistence.UserExistsAsync(baid, cancellationToken))
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

        _ = await persistence.GetOrCreateSaveAsync(baid, cancellationToken);
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
            await persistence.AddPlayRowAsync(row, cancellationToken);

            if (playResultData.PlayMode != (uint)PlayMode.DanMode || isShin)
            {
                await persistence.UpsertBestAsync(
                    baid,
                    new Ac15BestRow(stage.SongNo, difficulty, isShin, stage.PlayScore, stage.ScoreRate, crown),
                    bestPolicy,
                    cancellationToken);
            }

            await persistence.SetFavoriteAsync(baid, stage.SongNo, stage.IsFavorite, profile.Limits.MaxFavoriteSongs, cancellationToken);
            await persistence.UpsertRecentAsync(baid, stage.SongNo, playTime, cancellationToken);
        }

        await hooks.AfterNormalSaveAsync(new Ac15NormalSaveContext(baid, profile.Era, playResultData), cancellationToken);
        await persistence.SaveChangesAsync(cancellationToken);
        await persistence.TrimRecentAsync(baid, profile.Limits.MaxRecentSongs, cancellationToken);
        return 1;
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
}
