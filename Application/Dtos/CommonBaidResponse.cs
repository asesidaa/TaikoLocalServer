namespace TaikoLocalServer.Application.Dtos;

public partial class CommonBaidResponse
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
}
