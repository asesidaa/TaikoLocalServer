using SharedProject.Enums;

namespace GameDatabase.Entities;

public partial class ChallengeCompeteSongDatum
{
    public uint CompId { get; set; }
    public uint SongId { get; set; }
    public Difficulty Difficulty { get; set; }
    public uint? Speed { get; set; } = null;
    public bool? IsVanishOn { get; set; } = null;
    public bool? IsInverseOn { get; set; } = null;
    public RandomType? RandomType { get; set; } = null;
    public List<ChallengeCompeteBestDatum> BestScores { get; set; } = new();
    public virtual ChallengeCompeteDatum? ChallengeCompeteData { get; set; }
}
