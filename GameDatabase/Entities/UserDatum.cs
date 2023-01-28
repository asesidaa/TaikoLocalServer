using SharedProject.Enums;

namespace GameDatabase.Entities;

public class UserDatum
{
    public uint Baid { get; set; }
    public string MyDonName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public uint TitlePlateId { get; set; }
    public string FavoriteSongsArray { get; set; } = "[]";
    public string ToneFlgArray { get; set; } = "[]";
    public string TitleFlgArray { get; set; } = "[]";
    public string CostumeFlgArray { get; set; } = "[[],[],[],[],[]]";
    public string GenericInfoFlgArray { get; set; } = "[]";
    public short OptionSetting { get; set; }
    public int NotesPosition { get; set; }
    public bool IsVoiceOn { get; set; }
    public bool IsSkipOn { get; set; }
    public uint SelectedToneId { get; set; }
    public DateTime LastPlayDatetime { get; set; }
    public uint LastPlayMode { get; set; }
    public uint ColorBody { get; set; }
    public uint ColorFace { get; set; }
    public uint ColorLimb { get; set; }
    public string CostumeData { get; set; } = "[[],[],[],[],[]]";
    public bool DisplayDan { get; set; }
    public bool DisplayAchievement { get; set; }
    public Difficulty AchievementDisplayDifficulty { get; set; }

    public int AiWinCount { get; set; }

    public int TotalGameCount { get; set; }
    public virtual Card? Ba { get; set; }
}