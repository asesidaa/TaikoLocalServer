namespace TaikoLocalServer.Application.Handlers;

public partial class GetSelfBestQueryHandler
{
    private partial async ValueTask<CommonSelfBestResponse> HandleGreen(
        GetSelfBestQuery request,
        CancellationToken cancellationToken)
    {
        var difficulty = GreenPlayResultMapping.MapDifficulty(request.Difficulty);
        var requestedSongs = request.SongIdList ?? [];
        var requestedSet = requestedSongs.ToHashSet();
        var bestRows = await context.SongBestDataGreen
            .Where(row => row.Baid == request.Baid
                && row.Difficulty == difficulty
                && requestedSet.Contains(row.SongId))
            .ToDictionaryAsync(row => row.SongId, cancellationToken);

        var normalRows = requestedSongs.Select(songNo =>
        {
            bestRows.TryGetValue(songNo, out var best);
            return new CommonSelfBestResponse.SelfBestData
            {
                SongNo = songNo,
                SelfBestScore = best?.BestScore ?? 0,
                SelfBestScoreRate = best?.BestRate ?? 0
            };
        }).ToList();

        var shinRows = requestedSongs.Select(songNo => new CommonSelfBestResponse.SelfBestData
        {
            SongNo = songNo
        }).ToList();

        return new CommonSelfBestResponse
        {
            Result = 1,
            Level = request.Difficulty,
            ArySelfbestScores = normalRows,
            AryShinSelfbestScores = shinRows
        };
    }
}
