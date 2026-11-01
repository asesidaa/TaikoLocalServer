namespace TaikoLocalServer.Contracts.AdminApi.Responses;

/// <summary>
/// A player across the whole server (the Stats page's You vs the server, Your standing and Next goals' closest places).
/// Charts and crowns come from SongBestData, the same rows as the leaderboards and the Crowns table.
/// </summary>
public class PlayerServerStats
{
    /// <summary>Players with at least one chart played.</summary>
    public int Players { get; set; }
    /// <summary>Place by charts cleared (ties share it); 0 = no charts played.</summary>
    public int Rank { get; set; }
    public PlayerTotals Mine { get; set; } = new();
    /// <summary>The other players' averages; zeros when there's nobody else.</summary>
    public PlayerTotals Average { get; set; } = new();
    /// <summary>Two players above, this player, two below, by charts cleared.</summary>
    public List<StandingRow> Neighbours { get; set; } = [];
    /// <summary>Charts where this player is #1, in the top 3, in the top 10 (ties share a place).</summary>
    public int First { get; set; }
    public int Top3 { get; set; }
    public int Top10 { get; set; }
    /// <summary>The smallest score gaps to the next place up, smallest first.</summary>
    public List<PlaceGoal> ClosestPlaces { get; set; } = [];
}

public class PlayerTotals
{
    public double Cleared { get; set; }
    /// <summary>Cleared charts per played chart, 0–100.</summary>
    public double ClearRate { get; set; }
    public double FullCombos { get; set; }
    /// <summary>Over all plays: (Good + OK/2) / notes, 0–100, as on the Stats card.</summary>
    public double Accuracy { get; set; }
    public double Credits { get; set; }
}

public class StandingRow
{
    public int Rank { get; set; }
    public uint Baid { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Cleared { get; set; }
}

public class PlaceGoal
{
    public uint SongId { get; set; }
    public Difficulty Difficulty { get; set; }
    /// <summary>Points needed to pass the player just above.</summary>
    public uint Gap { get; set; }
    public string Name { get; set; } = string.Empty;
    /// <summary>Their place, which passing them takes.</summary>
    public int Rank { get; set; }
}

/// <summary>
/// Play Data › Stats for one player, summed on the server: their bests (four fields each) and totals over their plays,
/// so the page doesn't download every play (megabytes for a heavy player).
/// </summary>
public class PlayerStatsSummary
{
    public List<StatsBest> Bests { get; set; } = [];
    public int Plays { get; set; }
    /// <summary>Plays in the last 7 days, today included.</summary>
    public int PlaysThisWeek { get; set; }
    public uint BestCombo { get; set; }
    public long Good { get; set; }
    public long Ok { get; set; }
    public long Bad { get; set; }
    /// <summary>Plays per day over the last 53 weeks; days without plays are left out.</summary>
    public List<DayPlays> Days { get; set; } = [];
    /// <summary>Plays per song, every difficulty (genres and Most played).</summary>
    public List<SongPlays> Songs { get; set; } = [];
    /// <summary>Charts without a full combo, by the fewest Bads in any play (that play's OKs alongside), fewest first.</summary>
    public List<FullComboGoal> ClosestFullCombos { get; set; } = [];
    /// <summary>The last 14 days' new bests, the latest per chart, newest first.</summary>
    public List<RecentBest> RecentBests { get; set; } = [];
}

public record StatsBest(uint SongId, Difficulty Difficulty, CrownType BestCrown, ScoreRank BestScoreRank);
public record DayPlays(DateTime Day, int Plays);
public record SongPlays(uint SongId, int Plays);
public record FullComboGoal(uint SongId, Difficulty Difficulty, uint Bad, uint Ok);
public record RecentBest(uint SongId, Difficulty Difficulty, DateTime PlayTime, uint Score, uint Gain, CrownType Crown);
