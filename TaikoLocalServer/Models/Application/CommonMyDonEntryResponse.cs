namespace TaikoLocalServer.Models.Application;

public class CommonMyDonEntryResponse
{
    public uint   Result            { get; set; }
    public uint   Baid              { get; set; }
    public string MydonName         { get; set; } = string.Empty;
    public uint   MydonNameLanguage { get; set; }
    public string AccessCode        { get; set; } = string.Empty;
    public uint      ComSvrResult                 { get; set; }
}