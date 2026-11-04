namespace TaikoLocalServer.Contracts.AdminApi.Responses;

/// <summary>Play Data › High Scores: one difficulty's bests, filtered and paged, plus the counts around them.</summary>
public class HighScoresPage
{
    public List<HighScoreRow> Items { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
    /// <summary>Charts played per difficulty (Easy … Ura Oni), for the tabs.</summary>
    public int[] PlayedPerDifficulty { get; set; } = new int[5];
    /// <summary>Charts of this difficulty on the server.</summary>
    public int Charts { get; set; }
    /// <summary>Not cleared, cleared, full combo, donderful: over the search and genre, whatever the crown filter. AI
    /// Battle: lost, won.</summary>
    public int[] Crowns { get; set; } = new int[4];
    /// <summary>When comparing: the other player's name and how you stand on the charts you've both played.</summary>
    public string? VsName { get; set; }
    public int VsAhead { get; set; }
    public int VsBehind { get; set; }
    public int VsBoth { get; set; }
}

public class HighScoreRow
{
    public uint SongId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public SongGenre Genre { get; set; }
    public int Stars { get; set; }
    public uint Score { get; set; }
    public CrownType Crown { get; set; }
    public ScoreRank Rank { get; set; }
    /// <summary>The best's accuracy, %.</summary>
    public double Accuracy { get; set; }
    public DateTime? SetOn { get; set; }
    public uint? VsScore { get; set; }
    public CrownType? VsCrown { get; set; }
    /// <summary>AI Battle: whether they beat AI-DON, and on how many of its sections.</summary>
    public AiResult? Ai { get; set; }
}
