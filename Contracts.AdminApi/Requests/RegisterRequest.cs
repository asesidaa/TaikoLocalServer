namespace TaikoLocalServer.Contracts.AdminApi.Requests;

public class RegisterRequest
{
    public string AccessCode { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DateTime LastPlayDateTime { get; set; }
    public string InviteCode { get; set; } = string.Empty;
    /// <summary>The one-time setup code from the console (Server mode, no admin yet): makes this account the admin.</summary>
    public string SetupCode { get; set; } = string.Empty;
}
