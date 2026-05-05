namespace TaikoLocalServer.Infrastructure.Identity.Settings;

public class AuthSettings
{
    public string JwtKey { get; set; } = string.Empty;

    public string JwtIssuer { get; set; } = string.Empty;

    public string JwtAudience { get; set; } = string.Empty;

    public bool AuthenticationRequired { get; set; }

    public bool OnlyAdmin { get; set; }

    public int BoundAccessCodeUpperLimit { get; set; } = 3;

    public bool RegisterWithLastPlayTime { get; set; }

    public bool AllowUserDelete { get; set; } = true;

    public bool AllowFreeProfileEditing { get; set; } = true;
}
