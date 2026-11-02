namespace TaikoLocalServer.Contracts.AdminApi.ViewModels;

/// <summary>A game the player has data in (a save or plays): GameInfo's name, how many plays, and the last one.</summary>
public sealed record UserGame(string Game, int Plays, DateTime? LastPlayed);
