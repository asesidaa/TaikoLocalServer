using TaikoLocalServer.Application.Ac15;

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
            .ToListAsync(cancellationToken);

        var canonicalRows = bestRows.Select(row => new Ac15BestRow(
            row.SongId,
            row.Difficulty,
            row.IsShin,
            row.BestScore,
            row.BestRate,
            row.BestCrown));

        return Ac15SelfBestService.BuildResponse(request.Difficulty, requestedSongs, canonicalRows);
    }
}
