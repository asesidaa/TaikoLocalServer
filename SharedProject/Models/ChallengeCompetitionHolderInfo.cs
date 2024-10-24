namespace SharedProject.Models;

public class ChallengeCompetitionHolderInfo
{
    public uint Baid { get; set; }
    public string MyDonName { get; set; } = string.Empty;
    public uint MyDonNameLanguage { get; set; }
    public string Title { get; set; } = string.Empty;
    public uint TitlePlateId { get; set; }
    public uint ColorBody { get; set; }
    public uint ColorFace { get; set; }
    public uint ColorLimb { get; set; }
    public uint CurrentKigurumi { get; set; }
    public uint CurrentHead { get; set; }
    public uint CurrentBody { get; set; }
    public uint CurrentFace { get; set; }
    public uint CurrentPuchi { get; set; }
}
