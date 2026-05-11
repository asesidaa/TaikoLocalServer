// ReSharper disable InconsistentNaming
namespace TaikoLocalServer.Application.Dtos;

public partial class CommonPlayResultData
{
    public List<uint> UraReleaseSongNoes     { get; set; } = [];
    public List<uint> GetGenericInfoNoes     { get; set; } = [];
    public uint       TournamentMode         { get; set; }
    public uint       DifficultyPlayedCourse { get; set; }
    public uint       DifficultyPlayedStar   { get; set; }
    public uint       DifficultyPlayedSort   { get; set; }
    public uint       IsRandomUsePlay        { get; set; }
    public string     InputMedian            { get; set; } = string.Empty;
    public string     InputVariance          { get; set; } = string.Empty;

    public partial class StageData
    {
        public uint  StageMode        { get; set; }
        public int   NotesPosition    { get; set; }
        public bool  IsVoiceOn        { get; set; }
        public bool  IsSkipOn         { get; set; }
        public bool  IsSkipUse        { get; set; }
        public uint? IsRandomUseStage { get; set; }
        public bool  IsPapamama       { get; set; }
    }
}
