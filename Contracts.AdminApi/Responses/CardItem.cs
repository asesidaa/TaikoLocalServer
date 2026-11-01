namespace TaikoLocalServer.Contracts.AdminApi.Responses;

/// <summary>One of a player's cards (Settings › Your cards).</summary>
public sealed record CardItem(string AccessCode, string Name, DateTime? LastScanAt);

public sealed record RenameCardRequest(string Name);
