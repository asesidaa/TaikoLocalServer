namespace TaikoLocalServer.Application.Ac15;

public static class Ac15SelfBestService
{
    public static CommonSelfBestResponse BuildResponse(
        uint requestedDifficulty,
        IReadOnlyList<uint> requestedSongs,
        IEnumerable<Ac15BestRow> bestRows)
    {
        var difficulty = MapProtocolDifficulty(requestedDifficulty);
        var rowsByKey = bestRows.ToDictionary(row => (row.SongId, row.Difficulty, row.IsShin));

        return new CommonSelfBestResponse
        {
            Result = 1,
            Level = requestedDifficulty,
            ArySelfbestScores = requestedSongs.Select(songNo => BuildRow(songNo, difficulty, isShin: false, rowsByKey)).ToList(),
            AryShinSelfbestScores = requestedSongs.Select(songNo => BuildRow(songNo, difficulty, isShin: true, rowsByKey)).ToList()
        };
    }

    public static Difficulty[] GetRequestedDifficulties(uint requestedDifficulty)
    {
        var difficulty = MapProtocolDifficulty(requestedDifficulty);
        return difficulty == Difficulty.Oni
            ? [Difficulty.Oni, Difficulty.UraOni]
            : [difficulty];
    }

    private static CommonSelfBestResponse.SelfBestData BuildRow(
        uint songNo,
        Difficulty difficulty,
        bool isShin,
        IReadOnlyDictionary<(uint SongId, Difficulty Difficulty, bool IsShin), Ac15BestRow> rowsByKey)
    {
        rowsByKey.TryGetValue((songNo, difficulty, isShin), out var best);
        rowsByKey.TryGetValue((songNo, Difficulty.UraOni, isShin), out var uraBest);

        return new CommonSelfBestResponse.SelfBestData
        {
            SongNo = songNo,
            SelfBestScore = best?.BestScore ?? 0,
            SelfBestScoreRate = best?.BestRate ?? 0,
            UraBestScore = difficulty == Difficulty.Oni ? uraBest?.BestScore ?? 0 : 0,
            UraBestScoreRate = difficulty == Difficulty.Oni ? uraBest?.BestRate ?? 0 : 0
        };
    }

    private static Difficulty MapProtocolDifficulty(uint difficulty) => difficulty switch
    {
        1 => Difficulty.Easy,
        2 => Difficulty.Normal,
        3 => Difficulty.Hard,
        4 => Difficulty.Oni,
        5 => Difficulty.UraOni,
        _ => Difficulty.None
    };
}
