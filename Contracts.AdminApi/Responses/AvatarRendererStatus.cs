namespace TaikoLocalServer.Contracts.AdminApi.Responses;

/// <param name="Enabled">The admin's switch.</param>
/// <param name="Browser">The Edge or Chrome it uses; null when none was found.</param>
/// <param name="Running">The browser is open (it closes after a few idle minutes).</param>
public sealed record AvatarRendererStatus(bool Enabled, string? Browser, bool Running, int Queued, int Rendered, int Failed,
    string? LastError, DateTime? LastRenderedAt);
