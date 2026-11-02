namespace TaikoLocalServer.Contracts.AdminApi.Requests;

/// <summary>A Green friend to add: the 20-digit access code on their card (spaces or dashes allowed).</summary>
public sealed record AddFriendRequest(string AccessCode);
