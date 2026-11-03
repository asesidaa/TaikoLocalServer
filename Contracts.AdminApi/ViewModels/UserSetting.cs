namespace TaikoLocalServer.Contracts.AdminApi.ViewModels;

public class UserSetting
{
    public uint Baid { get; set; }
    
    public uint ToneId { get; set; }

    public bool IsDisplayAchievement { get; set; }

    public bool IsDisplayDanOnNamePlate { get; set; }

    public bool IsDisplaySouUchi { get; set; }

    public uint DifficultySettingCourse { get; set; }

    public uint DifficultySettingStar { get; set; }

    public uint DifficultySettingSort { get; set; }

    public bool IsVoiceOn { get; set; }

    public bool IsSkipOn { get; set; }

    public Difficulty AchievementDisplayDifficulty { get; set; }

    public PlaySetting PlaySetting { get; set; } = new();

    public int NotesPosition { get; set; }

    public string MyDonName { get; set; } = string.Empty;

    public uint MyDonNameLanguage { get; set; }

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
    
    public List<uint> UnlockedTitle { get; set; } = new();

    public uint FaceColor { get; set; }

    public uint BodyColor { get; set; }

    public uint LimbColor { get; set; }
    
    public DateTime LastPlayDateTime { get; set; }

    /// <summary>12 digits; read-only (the server assigns it and ignores it on save).</summary>

    // Player card (WebUI only; stored in UserWebProfiles).
    /// <summary>Game palette index (0 = Don red).</summary>
    public uint CardColor { get; set; }

    /// <summary>Settings › Display's song title language, when set (else song titles follow MyDonNameLanguage).</summary>
    public uint? SongLanguage { get; set; }
    public CardPattern CardPattern { get; set; }
    /// <summary>Game palette index, or -1 for no fade.</summary>
    public int CardFade { get; set; } = -1;
    public CardAnimation CardAnimation { get; set; }
}