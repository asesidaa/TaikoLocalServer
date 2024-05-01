using SharedProject.Enums;

namespace SharedProject.Models;

public class SongHistoryData
{
    public uint SongId { get; set; }

    public SongGenre Genre { get; set; }

    public string MusicName { get; set; } = string.Empty;

    public string MusicArtist { get; set; } = string.Empty;

    public Difficulty Difficulty { get; set; }

    public int Stars { get; set; }

    public bool ShowDetails { get; set; }

    public uint Score { get; set; }

    public CrownType Crown { get; set; }

    public ScoreRank ScoreRank { get; set; }

    public DateTime PlayTime { get; set; }

    public bool IsFavorite { get; set; }

    public uint GoodCount { get; set; }

    public uint OkCount { get; set; }

    public uint MissCount { get; set; }

    public uint ComboCount { get; set; }

    public uint HitCount { get; set; }

    public uint DrumrollCount { get; set; }

    public uint SongNumber { get; set; }

    //public List<AiSectionBestData> AiSectionBestData { get; set; } = new();

    //public bool ShowAiData { get; set; }
}