namespace GameDatabase.Entities;

public partial class ChallengeCompeteParticipantDatum
{
    public uint CompId { get; set; }
    public uint Baid { get; set; }
    public bool IsActive { get; set; }
    public virtual ChallengeCompeteDatum? ChallengeCompeteData { get; set; }
    public virtual UserDatum? UserData { get; set; }
}
