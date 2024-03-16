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
        public List<uint> FavoriteSongsArray  { get; set; } = [];
        public List<uint> ToneFlgArray        { get; set; } = [0];
        public List<uint> TitleFlgArray       { get; set; } = [];
        public string CostumeFlgArray     { get; set; } = "[[],[],[],[],[]]";
        public List<uint> UnlockedKigurumi { get; set; } = [0];
        public List<uint> UnlockedHead { get; set; } = [0];
        public List<uint> UnlockedBody { get; set; }= [0];
        public List<uint>     UnlockedFace { get; set; }= [0];
        public List<uint> UnlockedPuchi{ get; set; }= [0];
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
        public List<uint>      UnlockedSongIdList           { get; set; } = [];
        public bool        IsAdmin                      { get; set; }
    }
}