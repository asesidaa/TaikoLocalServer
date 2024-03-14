using SharedProject.Enums;

namespace GameDatabase.Entities
{
    public partial class UserDatum
    {
        public uint   Baid                { get; set; }
        public string MyDonName           { get; set; } = string.Empty;
        public uint   MyDonNameLanguage   { get; set; }
        public string Title               { get; set; } = string.Empty;
        public uint   TitlePlateId        { get; set; }
        public uint[] FavoriteSongsArray  { get; set; } = Array.Empty<uint>();
        public uint[] ToneFlgArray        { get; set; } = Array.Empty<uint>();
        public uint[] TitleFlgArray       { get; set; } = Array.Empty<uint>();
        public string CostumeFlgArray     { get; set; } = "[[],[],[],[],[]]";
        public uint[] GenericInfoFlgArray { get; set; } = Array.Empty<uint>();
        public short  OptionSetting       { get; set; }
        public int    NotesPosition       { get; set; }
        public bool   IsVoiceOn           { get; set; }
        public bool   IsSkipOn            { get; set; }
        // TODO: Split into separate fields
        public string       DifficultyPlayedArray        { get; set; } = "[]";
        // TODO: Split into separate fields
        public string       DifficultySettingArray       { get; set; } = "[]";
        public uint         SelectedToneId               { get; set; }
        public DateTime     LastPlayDatetime             { get; set; }
        public uint         LastPlayMode                 { get; set; }
        public uint         ColorBody                    { get; set; }
        public uint         ColorFace                    { get; set; }
        public uint         ColorLimb                    { get; set; }
        // TODO: Split into separate fields
        public string      CostumeData                  { get; set; } = "[]";
        public bool        DisplayDan                   { get; set; }
        public bool        DisplayAchievement           { get; set; }
        public Difficulty  AchievementDisplayDifficulty { get; set; }
        public int         AiWinCount                   { get; set; }
        public List<Token> Tokens                       { get; set; } = new();
        public uint[]      UnlockedSongIdList           { get; set; } = Array.Empty<uint>();
        public bool        IsAdmin                      { get; set; }
    }
}