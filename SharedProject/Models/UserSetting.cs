using SharedProject.Enums;

namespace SharedProject.Models;

public class UserSetting
{
    public uint ToneId { get; set; }

    public bool IsDisplayAchievement { get; set; }
    
    public bool IsDisplayDanOnNamePlate { get; set; }
    
    public bool IsVoiceOn { get; set; }
    
    public bool IsSkipOn { get; set; }

    public Difficulty AchievementDisplayDifficulty { get; set; }

    public PlaySetting PlaySetting { get; set; } = new();
    
    public int NotesPosition { get; set; }

    public string MyDonName { get; set; } = String.Empty;

    public string Title { get; set; } = String.Empty;

    public uint TitlePlateId { get; set; }
}