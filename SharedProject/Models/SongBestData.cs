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
}