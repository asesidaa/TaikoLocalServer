using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Domain.Entities;

/// <summary>One Web UI sign-in attempt (Access › Sign-ins). Kept 90 days; the access code is stored masked.</summary>
public class SignInAttempt
{
    public long Id { get; set; }
    public DateTime Time { get; set; }
    /// <summary>The account it was for, when the access code was known.</summary>
    public uint? Baid { get; set; }
    public string AccessCode { get; set; } = string.Empty;
    public SignInOutcome Result { get; set; }
    public string Ip { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}
