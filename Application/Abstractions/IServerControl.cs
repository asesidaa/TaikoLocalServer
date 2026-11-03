using System.Text.Json.Nodes;

namespace TaikoLocalServer.Application.Abstractions;

/// <summary>The setup wizard's and the game folder picker's hold on the running server: its config file, and a restart.</summary>
public interface IServerControl
{
    /// <summary>
    /// Changes the running server's Configurations/ServerSettings.json (the <c>ServerSettings</c> object), keeping the
    /// rest of the file. It's read at start-up, so a change only applies after a restart unless the caller applies it too.
    /// </summary>
    Task UpdateSettingsAsync(Action<JsonObject> change, CancellationToken cancellationToken);

    /// <summary>Stops the server and starts it again, the same way it was started (a few seconds).</summary>
    void Restart();
}
