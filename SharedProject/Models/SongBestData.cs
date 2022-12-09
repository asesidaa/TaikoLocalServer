using SharedProject.Enums;

namespace SharedProject.Models;

public class SongBestData
{
    public uint SongId { get; set; }

    public SongGenre Genre { get; set; }

    public string MusicName { get; set; } = string.Empty;

    public string MusicArtist { get; set; } = string.Empty;

    public Difficulty Difficulty { get; set; }

    public int PlayCount { get; set; }
    public int ClearCount { get; set; }
    public int FullComboCount { get; set; }
    public int PerfectCount { get; set; }

    public uint BestScore { get; set; }

    public uint BestRate { get; set; }

    public CrownType BestCrown { get; set; }

    public ScoreRank BestScoreRank { get; set; }

    public DateTime LastPlayTime { get; set; }

    public bool IsFavorite { get; set; }

    public uint GoodCount { get; set; }

    public uint OkCount { get; set; }

    public uint MissCount { get; set; }

    public uint ComboCount { get; set; }

    public uint HitCount { get; set; }

    public uint DrumrollCount { get; set; }

    public List<AiSectionBestData> AiSectionBestData { get; set; } = new();

    public bool ShowAiData { get; set; }
    
    public short Option { get; set; }
}