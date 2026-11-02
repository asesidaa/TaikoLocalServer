namespace TaikoLocalServer.Contracts.AdminApi.Responses;

/// <summary>Play History's calendar: plays per day of one month (after the filters).</summary>
public class PlayHistoryMonth
{
    /// <summary>Day of the month → plays that day; days without plays are left out.</summary>
    public Dictionary<int, int> Days { get; set; } = [];
    public int Plays { get; set; }
    /// <summary>The newest matching play's day, over all time (where the page opens); null when nothing matches.</summary>
    public DateTime? Latest { get; set; }
}

/// <summary>One day's plays, grouped into sessions (newest first).</summary>
public class PlayHistoryDay
{
    public List<PlaySession> Sessions { get; set; } = [];
}

/// <summary>Plays on one visit: back to back, under 30 minutes apart.</summary>
public class PlaySession
{
    /// <summary>The game it was played on (GameInfo's name). An older game's plays carry only IDs: the Web UI names them.</summary>
    public string Game { get; set; } = "Nijiiro";
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public int Credits { get; set; }
    /// <summary>Every song played in the session, whatever the filters.</summary>
    public int Songs { get; set; }
    /// <summary>The plays that match the filters: the newest credit first, each credit's songs in the order played.</summary>
    public List<HistoryPlay> Plays { get; set; } = [];
}

public class HistoryPlay
{
    public uint SongId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public Difficulty Difficulty { get; set; }
    public int Stars { get; set; }
    public DateTime PlayTime { get; set; }
    public uint Score { get; set; }
    public CrownType Crown { get; set; }
    public ScoreRank Rank { get; set; }
    /// <summary>Beat every earlier play of the chart (a first play isn't counted).</summary>
    public bool NewBest { get; set; }
    /// <summary>Which credit of its session it was played in, from 1.</summary>
    public int Credit { get; set; }
}

/// <summary>The Dashboard's server strip (admins).</summary>
public sealed record DashboardServer(ServerState State, int CabinetsOnline, int Cabinets, int PlaysToday);
