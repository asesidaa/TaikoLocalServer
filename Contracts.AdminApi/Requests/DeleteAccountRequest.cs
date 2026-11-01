namespace TaikoLocalServer.Contracts.AdminApi.Requests;

/// <summary>A player deleting their own account (api/Auth/DeleteAccount): their password, and a 2-step code if it's on.</summary>
public class DeleteAccountRequest
{
    public string Password { get; set; } = string.Empty;
    /// <summary>From the authenticator app, or a recovery code; needed only when 2-step verification is on.</summary>
    public string? Code { get; set; }
}
