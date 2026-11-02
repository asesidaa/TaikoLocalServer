namespace TaikoLocalServer.Adapters.AdminApi.Authorization;

/// <summary>
/// Which games a player has data in, and removing one of their scores in a game. The card and User ID are shared by
/// every game; scores, saves and profiles are kept per game (each game's tables, all cascading from UserDatum).
/// </summary>
public static class UserGames
{
    /// <summary>For each of <paramref name="baids"/>, the games they have a save or plays in, newest first.</summary>
    public static async Task<Dictionary<uint, List<UserGame>>> ForAsync(ITaikoDbContext c, IReadOnlyCollection<uint> baids, CancellationToken ct)
    {
        var result = baids.ToDictionary(b => b, _ => new List<UserGame>());
        async Task Add(string game, IQueryable<GamePlays> plays, IQueryable<uint> saved)
        {
            var counts = await plays.ToDictionaryAsync(p => p.Baid, ct);
            foreach (var baid in (await saved.ToListAsync(ct)).Union(counts.Keys))
                result[baid].Add(counts.TryGetValue(baid, out var p) ? new UserGame(game, p.Count, p.Last) : new UserGame(game, 0, null));
        }

        await Add("Nijiiro",
            c.SongPlayDataNijiiro.Where(p => baids.Contains(p.Baid)).GroupBy(p => p.Baid).Select(g => new GamePlays(g.Key, g.Count(), g.Max(p => p.PlayTime))),
            c.UserSaveDataNijiiro.Where(s => baids.Contains(s.Baid)).Select(s => s.Baid));
        await Add("Green", Plays(c.SongPlayDataGreen, baids), c.UserSaveDataGreen.Where(s => baids.Contains(s.Baid)).Select(s => s.Baid));
        await Add("Blue", Plays(c.SongPlayDataBlue, baids), c.UserSaveDataBlue.Where(s => baids.Contains(s.Baid)).Select(s => s.Baid));
        await Add("Yellow", Plays(c.SongPlayDataYellow, baids), c.UserSaveDataYellow.Where(s => baids.Contains(s.Baid)).Select(s => s.Baid));
        await Add("Red", Plays(c.SongPlayDataRed, baids), c.UserSaveDataRed.Where(s => baids.Contains(s.Baid)).Select(s => s.Baid));
        await Add("White", Plays(c.SongPlayDataWhite, baids), c.UserSaveDataWhite.Where(s => baids.Contains(s.Baid)).Select(s => s.Baid));
        await Add("Murasaki", Plays(c.SongPlayDataMurasaki, baids), c.UserSaveDataMurasaki.Where(s => baids.Contains(s.Baid)).Select(s => s.Baid));
        await Add("Kimidori", Plays(c.SongPlayDataKimidori, baids), c.UserSaveDataKimidori.Where(s => baids.Contains(s.Baid)).Select(s => s.Baid));
        await Add("Momoiro", Plays(c.SongPlayDataMomoiro, baids), c.UserSaveDataMomoiro.Where(s => baids.Contains(s.Baid)).Select(s => s.Baid));

        foreach (var list in result.Values) list.Sort((a, b) => Nullable.Compare(b.LastPlayed, a.LastPlayed));
        return result;
    }

    /// <summary>
    /// Removes an older game's best score (both rows on games with Shin charts) and the plays behind it. Null when there's
    /// no such score; Nijiiro's is UsersController.RemoveScore.
    /// </summary>
    public static Task<(uint BestScore, int Plays)?> RemoveScoreAsync(ITaikoDbContext c, GameEra era, uint baid, uint songId, Difficulty difficulty, CancellationToken ct) => era switch
    {
        GameEra.Green => Remove(c.SongBestDataGreen, c.SongPlayDataGreen, baid, songId, difficulty, ct),
        GameEra.Blue => Remove(c.SongBestDataBlue, c.SongPlayDataBlue, baid, songId, difficulty, ct),
        GameEra.Yellow => Remove(c.SongBestDataYellow, c.SongPlayDataYellow, baid, songId, difficulty, ct),
        GameEra.Red => Remove(c.SongBestDataRed, c.SongPlayDataRed, baid, songId, difficulty, ct),
        GameEra.White => Remove(c.SongBestDataWhite, c.SongPlayDataWhite, baid, songId, difficulty, ct),
        GameEra.Murasaki => Remove(c.SongBestDataMurasaki, c.SongPlayDataMurasaki, baid, songId, difficulty, ct),
        GameEra.Kimidori => Remove(c.SongBestDataKimidori, c.SongPlayDataKimidori, baid, songId, difficulty, ct),
        GameEra.Momoiro => Remove(c.SongBestDataMomoiro, c.SongPlayDataMomoiro, baid, songId, difficulty, ct),
        _ => throw new ArgumentOutOfRangeException(nameof(era)),
    };

    private sealed record GamePlays(uint Baid, int Count, DateTime Last);

    private static IQueryable<GamePlays> Plays<T>(IQueryable<T> plays, IReadOnlyCollection<uint> baids) where T : class, IAc15SongPlayDatum =>
        plays.Where(p => baids.Contains(p.Baid)).GroupBy(p => p.Baid).Select(g => new GamePlays(g.Key, g.Count(), g.Max(p => p.PlayTime)));

    private static async Task<(uint BestScore, int Plays)?> Remove<TBest, TPlay>(DbSet<TBest> bests, DbSet<TPlay> plays, uint baid, uint songId, Difficulty difficulty, CancellationToken ct)
        where TBest : class, IAc15SongBestDatum where TPlay : class, IAc15SongPlayDatum
    {
        var bestRows = await bests.Where(b => b.Baid == baid && b.SongId == songId && b.Difficulty == difficulty).ToListAsync(ct);
        if (bestRows.Count == 0) return null;
        var playRows = await plays.Where(p => p.Baid == baid && p.SongId == songId && p.Difficulty == difficulty).ToListAsync(ct);
        bests.RemoveRange(bestRows);
        plays.RemoveRange(playRows);
        return (bestRows.Max(b => b.BestScore), playRows.Count);
    }
}
