namespace TaikoLocalServer.Contracts.AdminApi.Requests;

public class RegisterRequest
{
    public string AccessCode { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DateTime LastPlayDateTime { get; set; }
    public string InviteCode { get; set; } = string.Empty;
}
