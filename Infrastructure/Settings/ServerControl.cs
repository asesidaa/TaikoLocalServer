using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Infrastructure.Identity;

namespace TaikoLocalServer.Infrastructure.Settings;

/// <summary>
/// <see cref="IServerControl"/>: edits Configurations/ServerSettings.json next to the running exe (written to a temp file
/// first, so a crash never leaves half a file), and restarts by starting the same exe with the same arguments once the
/// ports are free. A pending setup code goes along (<see cref="ServerAccess.SetupCodeVariable"/>), so the wizard carries on.
/// </summary>
public sealed class ServerControl(IHostApplicationLifetime lifetime, IServerAccess access, ILogger<ServerControl> logger) : IServerControl
{
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static string SettingsPath => Path.Combine(AppContext.BaseDirectory, "Configurations", "ServerSettings.json");
    private int restarting;

    public async Task UpdateSettingsAsync(Action<JsonObject> change, CancellationToken cancellationToken)
    {
        await Gate.WaitAsync(cancellationToken);
        try
        {
            var root = JsonNode.Parse(await File.ReadAllTextAsync(SettingsPath, cancellationToken), documentOptions:
                new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true }) as JsonObject ?? [];
            if (root["ServerSettings"] is not JsonObject settings)
                root["ServerSettings"] = settings = [];
            change(settings);
            var temp = SettingsPath + ".tmp";
            await File.WriteAllTextAsync(temp, root.ToJsonString(new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            }), cancellationToken);
            File.Move(temp, SettingsPath, overwrite: true);
        }
        finally
        {
            Gate.Release();
        }
    }

    public void Restart()
    {
        if (Interlocked.Exchange(ref restarting, 1) == 1) return;
        var exe = Environment.ProcessPath ?? throw new InvalidOperationException("The server's own path isn't known.");
        // `dotnet TaikoLocalServer.dll …` passes the dll as the first argument; the exe doesn't.
        var args = Environment.GetCommandLineArgs().Skip(Path.GetFileNameWithoutExtension(exe) == "dotnet" ? 0 : 1).ToList();
        var code = access.SetupCode;
        lifetime.ApplicationStopped.Register(() =>
        {
            var start = new ProcessStartInfo(exe) { UseShellExecute = false, WorkingDirectory = Environment.CurrentDirectory };
            foreach (var arg in args) start.ArgumentList.Add(arg);
            if (code is not null) start.Environment[ServerAccess.SetupCodeVariable] = code;
            Process.Start(start);
        });
        logger.LogWarning("Restarting to apply the new settings…");
        // A moment for the response that asked for it to reach the browser.
        _ = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            lifetime.StopApplication();
        });
    }
}
