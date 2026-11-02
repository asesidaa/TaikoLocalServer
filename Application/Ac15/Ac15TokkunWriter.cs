using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15TokkunTables<TRecent>(
    DbSet<TRecent> RecentRows,
    Action<uint, Ac15PlayResultEnvelope, Ac15TokkunStageData>? AddStageHistory = null)
    where TRecent : class, IAc15RecentSong, new();

public sealed record Ac15TokkunWriteRequest(
    uint Baid,
    Ac15PlayResultEnvelope PlayResultData,
    uint? CurrentTutorialFlg,
    Action<uint?> SetTutorialFlg,
    int MaxRecentSongs,
    DateTime PlayTime);

public static class Ac15TokkunWriter
{
    public static async ValueTask SaveAsync<TRecent>(
        ITaikoDbContext context,
        Ac15TokkunTables<TRecent> tables,
        Ac15TokkunWriteRequest request,
        CancellationToken cancellationToken)
        where TRecent : class, IAc15RecentSong, new()
    {
        request.SetTutorialFlg(Ac15CommonProfileMutation.PreserveTutorialFlag(
            request.CurrentTutorialFlg,
            request.PlayResultData.Tokkun?.TutorialFlg));

        var wroteRecentSongs = false;
        if (request.PlayResultData.Tokkun?.StageData is { } tokkunStageData)
        {
            tables.AddStageHistory?.Invoke(request.Baid, request.PlayResultData, tokkunStageData);
            foreach (var songNo in tokkunStageData.TookunSongnoes)
            {
                await Ac15NormalPlayWriter.UpsertRecentAsync(
                    tables.RecentRows,
                    request.Baid,
                    songNo,
                    request.PlayTime,
                    cancellationToken);
                wroteRecentSongs = true;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        if (!wroteRecentSongs)
        {
            return;
        }

        await Ac15NormalPlayWriter.TrimRecentAsync(
            tables.RecentRows,
            context.SaveChangesAsync,
            request.Baid,
            request.MaxRecentSongs,
            cancellationToken);
    }
}
