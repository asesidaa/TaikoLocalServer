namespace TaikoLocalServer.Entities
{
    public partial class UserDatum
    {
        public uint Baid { get; set; }
        public string MyDonName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public uint TitlePlateId { get; set; }
        public string FavoriteSongsArray { get; set; } = string.Empty;
        public string ToneFlgArray { get; set; } = string.Empty;
        public string TitleFlgArray { get; set; } = string.Empty;
        public string CostumeFlgArray { get; set; } = string.Empty;
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
        public string CostumeData { get; set; } = string.Empty;
        public bool DisplayDan { get; set; }
        public bool DisplayAchievement { get; set; }
        public Difficulty AchievementDisplayDifficulty { get; set; }

        public virtual Card? Ba { get; set; }
    }
}
