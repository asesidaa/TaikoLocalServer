namespace TaikoLocalServer.Settings;

public class AuthSettings
{
    public string JwtKey { get; set; } = string.Empty;

    public string JwtIssuer { get; set; } = string.Empty;

    public string JwtAudience { get; set; } = string.Empty;
    
    public bool LoginRequired { get; set; }
}