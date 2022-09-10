using SharedProject.Enums;

namespace SharedProject.Models;

public class SongBestData
{
    public uint SongId { get; set; }
    
    public Difficulty Difficulty { get; set; }
    
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
}