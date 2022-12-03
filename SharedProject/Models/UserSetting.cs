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

    public string MyDonName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public uint TitlePlateId { get; set; }

    public uint Kigurumi { get; set; }

    public uint Head { get; set; }

    public uint Body { get; set; }

    public uint Face { get; set; }

    public uint Puchi { get; set; }

    public List<uint> UnlockedKigurumi { get; set; } = new();

    public List<uint> UnlockedHead { get; set; } = new();

    public List<uint> UnlockedBody { get; set; } = new();

    public List<uint> UnlockedFace { get; set; } = new();

    public List<uint> UnlockedPuchi { get; set; } = new();

    public uint FaceColor { get; set; }

    public uint BodyColor { get; set; }

    public uint LimbColor { get; set; }
}