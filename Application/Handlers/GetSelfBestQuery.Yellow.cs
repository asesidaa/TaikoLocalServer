using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetSelfBestQueryHandler
{
    private partial async ValueTask<CommonSelfBestResponse> HandleYellow(
        GetSelfBestQuery request,
        CancellationToken cancellationToken)
    {
        var difficulty = MapYellowDifficulty(request.Difficulty);
        var requestedSongs = request.SongIdList ?? [];
        var requestedSet = requestedSongs.ToHashSet();
        var bestRows = await context.SongBestDataYellow
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

    private static Difficulty MapYellowDifficulty(uint level) => level switch
    {
        1 => Difficulty.Easy,
        2 => Difficulty.Normal,
        3 => Difficulty.Hard,
        4 => Difficulty.Oni,
        5 => Difficulty.UraOni,
        _ => Difficulty.None
    };
}
