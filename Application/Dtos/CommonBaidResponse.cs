namespace TaikoLocalServer.Application.Dtos;

public partial class CommonBaidResponse
{
    public uint Result { get; set; }
    public bool       IsNewUser                 { get;   set; }
    public uint       Baid              { get;   set; }
    public string     MyDonName         { get;   set; } = string.Empty;
    public uint       MyDonNameLanguage { get;   set; }
}
