using SharedProject.Enums;

namespace GameDatabase.Entities;

public partial class ChallengeCompeteSongDatum
{
    public uint CompId { get; set; }
    public uint SongId { get; set; }
    public Difficulty Difficulty { get; set; }
    public short SongOpt { get; set; }
    public List<ChallengeCompeteBestDatum> BestScores { get; set; } = new();
    public virtual ChallengeCompeteDatum? ChallengeCompeteData { get; set; }
}
