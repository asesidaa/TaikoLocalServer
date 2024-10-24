using SharedProject.Enums;

namespace GameDatabase.Entities;

public partial class ChallengeCompeteDatum
{
    public uint CompId { get; set; }
    public CompeteModeType CompeteMode { get; set; } = CompeteModeType.None;
    public CompeteState State { get; set; } = CompeteState.Normal;
    public uint Baid { get; set; }
    public string CompeteName { get; set; } = String.Empty;
    public string CompeteDescribe { get; set; } = String.Empty;
    public uint MaxParticipant { get; set; } = 2;
    public DateTime CreateTime { get; set; }
    public DateTime ExpireTime { get; set; }
    public uint RequireTitle { get; set; } = 0;
    public bool OnlyPlayOnce { get; set; } = false;
    public ShareType Share { get; set; } = ShareType.EveryOne;
    public CompeteTargetType CompeteTarget { get; set; } = CompeteTargetType.EveryOne;
    public List<ChallengeCompeteSongDatum> Songs { get; set; } = new();
    public List<ChallengeCompeteParticipantDatum> Participants { get; set; } = new();
}

