namespace TaikoLocalServer.Contracts.AdminApi.Responses;

/// <summary>One page of Play Data › Song List: every song on the server, with this player's results.</summary>
public class SongListPage
{
    public List<SongListItem> Items { get; set; } = [];
    /// <summary>Songs matching the filters.</summary>
    public int Total { get; set; }
    /// <summary>Songs on the server, for the search placeholder.</summary>
    public int AllSongs { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
}

public class SongListItem
{
    public uint SongId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public SongGenre Genre { get; set; }
    public bool Favorite { get; set; }
    /// <summary>Plays of this song by every player on the server (skipped songs don't count).</summary>
    public int TotalPlays { get; set; }
    /// <summary>Easy, Normal, Hard, Oni, Ura Oni; 0 = no chart.</summary>
    public int[] Stars { get; set; } = new int[5];
    /// <summary>Per difficulty, the best crown; null = not played.</summary>
    public CrownType?[] Crowns { get; set; } = new CrownType?[5];
    /// <summary>Per difficulty, the AI Battle result; null = no AI battle yet.</summary>
    public AiResult?[] Ai { get; set; } = new AiResult?[5];
}

public class AiResult
{
    public bool Win { get; set; }
    public int SectionsWon { get; set; }
    public int Sections { get; set; }
}

/// <summary>The Song page: the song and, per chart, this player's best, every play and the AI Battle.</summary>
public class SongDetail
{
    public uint SongId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public SongGenre Genre { get; set; }
    public bool Favorite { get; set; }
    /// <summary>Plays of this song by every player on the server (skipped songs don't count).</summary>
    public int TotalPlays { get; set; }
    public int[] Stars { get; set; } = new int[5];
    /// <summary>One per difficulty that has a chart, Easy first.</summary>
    public List<SongChart> Charts { get; set; } = [];
}

public class SongChart
{
    public Difficulty Difficulty { get; set; }
    public int Stars { get; set; }
    /// <summary>Every play, oldest first.</summary>
    public List<SongChartPlay> Plays { get; set; } = [];
    public List<AiSection>? AiSections { get; set; }
    public bool? AiWin { get; set; }
}

public class SongChartPlay
{
    public DateTime PlayTime { get; set; }
    public uint Score { get; set; }
    public CrownType Crown { get; set; }
    public ScoreRank Rank { get; set; }
    public uint Good { get; set; }
    public uint Ok { get; set; }
    public uint Bad { get; set; }
    public uint Combo { get; set; }
    public uint Drumroll { get; set; }
}

public class AiSection
{
    public CrownType Crown { get; set; }
    public bool Win { get; set; }
    public uint Score { get; set; }
    public uint Good { get; set; }
    public uint Ok { get; set; }
    public uint Bad { get; set; }
    public uint Drumroll { get; set; }
}
