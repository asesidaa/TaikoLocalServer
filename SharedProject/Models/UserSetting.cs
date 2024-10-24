using SharedProject.Enums;
using System.Text.Json.Serialization;

namespace SharedProject.Models;

public class UserSetting
{
    [JsonPropertyName("baid")]
    public uint Baid { get; set; }

    [JsonPropertyName("toneId")]
    public uint ToneId { get; set; }

    [JsonPropertyName("isDisplayAchievement")]
    public bool IsDisplayAchievement { get; set; }

    [JsonPropertyName("isDisplayDanOnNamePlate")]
    public bool IsDisplayDanOnNamePlate { get; set; }

    [JsonPropertyName("difficultySettingCourse")]
    public uint DifficultySettingCourse { get; set; }

    [JsonPropertyName("difficultySettingStar")]
    public uint DifficultySettingStar { get; set; }

    [JsonPropertyName("difficultySettingSort")]
    public uint DifficultySettingSort { get; set; }

    [JsonPropertyName("isVoiceOn")]
    public bool IsVoiceOn { get; set; }

    [JsonPropertyName("isSkipOn")]
    public bool IsSkipOn { get; set; }

    [JsonPropertyName("achievementDisplayDifficulty")]
    public Difficulty AchievementDisplayDifficulty { get; set; }

    [JsonPropertyName("playSetting")]
    public PlaySetting PlaySetting { get; set; } = new();

    [JsonPropertyName("notesPosition")]
    public int NotesPosition { get; set; }

    [JsonPropertyName("myDonName")]
    public string MyDonName { get; set; } = string.Empty;

    [JsonPropertyName("myDonNameLanguage")]
    public uint MyDonNameLanguage { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("titlePlateId")]
    public uint TitlePlateId { get; set; }

    [JsonPropertyName("kigurumi")]
    public uint Kigurumi { get; set; }

    [JsonPropertyName("head")]
    public uint Head { get; set; }

    [JsonPropertyName("body")]
    public uint Body { get; set; }

    [JsonPropertyName("face")]
    public uint Face { get; set; }

    [JsonPropertyName("puchi")]
    public uint Puchi { get; set; }

    [JsonPropertyName("unlockedKigurumi")]
    public List<uint> UnlockedKigurumi { get; set; } = new();

    [JsonPropertyName("unlockedHead")]
    public List<uint> UnlockedHead { get; set; } = new();

    [JsonPropertyName("unlockedBody")]
    public List<uint> UnlockedBody { get; set; } = new();

    [JsonPropertyName("unlockedFace")]
    public List<uint> UnlockedFace { get; set; } = new();

    [JsonPropertyName("unlockedPuchi")]
    public List<uint> UnlockedPuchi { get; set; } = new();

    [JsonPropertyName("unlockedTitle")]
    public List<uint> UnlockedTitle { get; set; } = new();

    [JsonPropertyName("faceColor")]
    public uint FaceColor { get; set; }

    [JsonPropertyName("bodyColor")]
    public uint BodyColor { get; set; }

    [JsonPropertyName("limbColor")]
    public uint LimbColor { get; set; }

    [JsonPropertyName("lastPlayDateTime")]
    public DateTime LastPlayDateTime { get; set; }
}