namespace TaikoLocalServer.Contracts.AdminApi.ViewModels;

/// <summary>The signed-in user's 2-step verification (Settings › Security).</summary>
public class TwoFactorStatus
{
    public TwoFactorMode Mode { get; set; }
    public bool Enabled { get; set; }
    public int RecoveryCodesLeft { get; set; }
}

/// <summary>A new authenticator key; nothing is stored until it's confirmed with a code.</summary>
public class TwoFactorSetup
{
    /// <summary>Base32, what an authenticator app asks for when typed in.</summary>
    public string Secret { get; set; } = string.Empty;
    /// <summary>The otpauth:// URI a QR code would carry.</summary>
    public string Uri { get; set; } = string.Empty;
}

/// <summary>Enable: the key from Setup plus a code from the app. Disable / new recovery codes: a code (or a recovery code).</summary>
public class TwoFactorCodeRequest
{
    public string Secret { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

/// <summary>Shown once. A new sign-in token comes back when this unlocked admin access.</summary>
public class TwoFactorCodesResponse
{
    public List<string> RecoveryCodes { get; set; } = [];
    public string? AuthToken { get; set; }
}

/// <summary>Login step 2: the token from step 1 and a 6-digit or recovery code.</summary>
public class VerifyTwoFactorRequest
{
    public string PendingToken { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
