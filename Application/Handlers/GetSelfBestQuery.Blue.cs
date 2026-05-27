namespace TaikoLocalServer.Application.Handlers;

public partial class GetSelfBestQueryHandler
{
    private partial async ValueTask<CommonSelfBestResponse> HandleBlue(
        GetSelfBestQuery request,
        CancellationToken cancellationToken)
    {
        var difficulty = BluePlayResultMapping.MapDifficulty(request.Difficulty);
        var requestedSongs = request.SongIdList ?? [];
        var requestedSet = requestedSongs.ToHashSet();
        var bestRows = await context.SongBestDataBlue
            .Where(row => row.Baid == request.Baid
                && row.Difficulty == difficulty
                && requestedSet.Contains(row.SongId))
            .ToListAsync(cancellationToken);

        var normalRowsBySong = bestRows
            .Where(row => !row.IsShin)
            .ToDictionary(row => row.SongId);
        var shinRowsBySong = bestRows
            .Where(row => row.IsShin)
            .ToDictionary(row => row.SongId);

        return new CommonSelfBestResponse
        {
            Result = 1,
            Level = request.Difficulty,
            ArySelfbestScores = requestedSongs.Select(songNo =>
            {
                normalRowsBySong.TryGetValue(songNo, out var best);
                return new CommonSelfBestResponse.SelfBestData
                {
                    SongNo = songNo,
                    SelfBestScore = best?.BestScore ?? 0,
                    SelfBestScoreRate = best?.BestRate ?? 0
                };
            }).ToList(),
            AryShinSelfbestScores = requestedSongs.Select(songNo =>
            {
                shinRowsBySong.TryGetValue(songNo, out var best);
                return new CommonSelfBestResponse.SelfBestData
                {
                    SongNo = songNo,
                    SelfBestScore = best?.BestScore ?? 0,
                    SelfBestScoreRate = best?.BestRate ?? 0
                };
            }).ToList()
        };
    }
}
