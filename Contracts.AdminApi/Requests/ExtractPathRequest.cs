namespace TaikoLocalServer.Contracts.AdminApi.Requests;

/// <summary>Read <see cref="Game"/> from this folder on the server (its Eras.&lt;Game&gt;.GamePath; Nijiiro's GameDataPath).</summary>
public record ExtractPathRequest(string Game, string Path);
