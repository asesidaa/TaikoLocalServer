namespace TaikoLocalServer.Application.Ac15;

public static class Ac15SelfBestService
{
    public static CommonSelfBestResponse BuildResponse(
        uint requestedDifficulty,
        IReadOnlyList<uint> requestedSongs,
        IEnumerable<Ac15BestRow> bestRows)
    {
        var rows = bestRows.ToList();
        var normalRowsBySong = rows
            .Where(row => !row.IsShin)
            .ToDictionary(row => row.SongId);
        var shinRowsBySong = rows
            .Where(row => row.IsShin)
            .ToDictionary(row => row.SongId);

        return new CommonSelfBestResponse
        {
            Result = 1,
            Level = requestedDifficulty,
            ArySelfbestScores = requestedSongs.Select(songNo => BuildRow(songNo, normalRowsBySong)).ToList(),
            AryShinSelfbestScores = requestedSongs.Select(songNo => BuildRow(songNo, shinRowsBySong)).ToList()
        };
    }

    private static CommonSelfBestResponse.SelfBestData BuildRow(
        uint songNo,
        IReadOnlyDictionary<uint, Ac15BestRow> rowsBySong)
    {
        rowsBySong.TryGetValue(songNo, out var best);
        return new CommonSelfBestResponse.SelfBestData
        {
            SongNo = songNo,
            SelfBestScore = best?.BestScore ?? 0,
            SelfBestScoreRate = best?.BestRate ?? 0
        };
    }
}
