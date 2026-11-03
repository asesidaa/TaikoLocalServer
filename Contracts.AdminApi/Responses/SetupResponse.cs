namespace TaikoLocalServer.Contracts.AdminApi.Responses;

/// <summary>A setup step was saved. <see cref="Restarting"/>: the server is restarting to apply it (a few seconds).</summary>
public record SetupResponse(bool Restarting);
