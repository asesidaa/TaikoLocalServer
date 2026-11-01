namespace TaikoLocalServer.Contracts.AdminApi.Requests;

public class LoginRequest
{
    public string AccessCode { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    /// <summary>From an earlier "Trust this browser": skips the 2-step code while it's valid.</summary>
    public string? TrustToken { get; set; }
}