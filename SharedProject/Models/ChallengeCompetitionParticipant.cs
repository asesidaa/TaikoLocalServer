namespace SharedProject.Models;

public class ChallengeCompetitionParticipant
{
    public uint CompId { get; set; }
    public uint Baid { get; set; }
    public bool IsActive { get; set; }
    public UserAppearance? UserInfo { get; set; } = new();
}
