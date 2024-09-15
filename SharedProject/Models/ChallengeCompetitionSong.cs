using SharedProject.Enums;

namespace SharedProject.Models;

public class ChallengeCompetitionSong
{
    public uint CompId { get; set; }
    public uint SongId { get; set; }
    public MusicDetail? MusicDetail { get; set; } = null;
    public Difficulty Difficulty { get; set; }
    public uint? Speed { get; set; } = null;
    public bool? IsVanishOn { get; set; } = null;
    public bool? IsInverseOn { get; set; } = null;
    public RandomType? RandomType { get; set; } = null;
    public List<ChallengeCompetitionBestScore> BestScores { get; set; } = new();
}
