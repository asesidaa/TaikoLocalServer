namespace TaikoLocalServer.Application.Dtos;

public class CommonBaidResponse
{
    public uint Result { get; set; }
    /// <summary>
    /// WW 39.06 only (CHN's response has no such field). 300, 306 or 310 makes the cabinet refuse the card with
    /// "this card can't be used" (GameConnectStatus kError, kDataFail); null leaves it out of the response.
    /// </summary>
    public uint? ComSvrResult { get; set; }
    public bool       IsNewUser                 { get;   set; }
    public uint       Baid              { get;   set; }
    public string     MyDonName         { get;   set; } = string.Empty;
    public uint       MyDonNameLanguage { get;   set; }
    public string     Title             { get;   set; } = string.Empty;
    public uint       TitlePlateId      { get;   set; }
    public uint       ColorFace         { get;   set; }
    public uint       ColorBody         { get;   set; }
    public uint       ColorLimb         { get;   set; }
    public List<uint> CostumeData       { get;   set; } = new() { 0, 0, 0, 0, 0 };
    public List<byte[]> CostumeFlagArrays { get; set; } 
        = new() { Array.Empty<byte>(), Array.Empty<byte>(), Array.Empty<byte>(), Array.Empty<byte>(), Array.Empty<byte>() };

    public string LastPlayDatetime    { get; set; } = DateTime.Now.ToString(Constants.DateTimeFormat);
    public bool   DisplayDan          { get; set; }
    public bool   IsDispSouuchiOn { get; set; }
    public uint   GotDanMax           { get; set; }
    public byte[] GotDanFlg           { get; set; } = Array.Empty<byte>();
    public byte[] GotGaidenFlg        { get; set; } = Array.Empty<byte>();
    public uint   SelectedToneId      { get; set; }
    public byte[] GenericInfoFlg      { get; set; } = Array.Empty<byte>();
    public uint[] AryCrownCounts      { get; set; } = Array.Empty<uint>();
    public uint[] AryScoreRankCounts  { get; set; } = Array.Empty<uint>();
    public bool   IsDispAchievementOn { get; set; }
    public uint   DispAchievementType { get; set; }
    public bool   IsDispAchievementTypeSet { get; set; }
    public uint   LastPlayMode        { get; set; }
    public uint   AiRank              { get; set; }
    public uint   AiTotalWin                   { get; set; }
}