namespace TaikoLocalServer.Contracts.AdminApi.Requests;

public class BindAccessCodeRequest
{
    public string AccessCode { get; set; } = string.Empty;
    
    public uint Baid { get; set; }
}