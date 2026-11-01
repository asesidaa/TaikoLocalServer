using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Contracts.AdminApi.Responses;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog;

/// <summary>
/// Runs <c>extract(.exe)</c> from next to the server exe with <c>--game GamePath --out wwwroot</c>, captures its
/// report, and reloads the game data when it's done. One run at a time.
/// </summary>
public sealed class ExtractRunner(IOptions<ServerSettings> settings, IGameDataCatalog catalog, ILogger<ExtractRunner> logger) : IExtractRunner
{
    private const int MaxLines = 2000;

    private readonly Lock gate = new();
    private readonly List<string> output = [];
    private bool running, mods;
    private DateTime? startedAt, finishedAt;
    private int? exitCode;

    private static string ExePath => Path.Combine(Path.GetDirectoryName(Environment.ProcessPath)!, OperatingSystem.IsWindows() ? "extract.exe" : "extract");
    private string GamePath => settings.Value.GamePath?.Trim() ?? "";

    public ExtractStatus Status
    {
        get
        {
            lock (gate)
                return new ExtractStatus(File.Exists(ExePath), GamePath, GamePath.Length > 0 && Directory.Exists(GamePath),
                    running, mods, startedAt, finishedAt, exitCode, output.ToList());
        }
    }

    public string? Start(bool withMods)
    {
        if (!File.Exists(ExePath)) return "extract isn't next to the server exe.";
        if (GamePath.Length == 0 || !Directory.Exists(GamePath)) return "GamePath isn't set, or the folder doesn't exist.";

        var info = new ProcessStartInfo(ExePath)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        info.ArgumentList.Add("--game");
        info.ArgumentList.Add(GamePath);
        info.ArgumentList.Add("--out");
        info.ArgumentList.Add(PathHelper.GetRootPath());
        if (withMods) info.ArgumentList.Add("--mods");

        lock (gate)
        {
            if (running) return "extract is already running.";
            running = true;
            mods = withMods;
            startedAt = DateTime.UtcNow;
            finishedAt = null;
            exitCode = null;
            output.Clear();
        }
        _ = Task.Run(() => RunAsync(info));
        return null;
    }

    private async Task RunAsync(ProcessStartInfo info)
    {
        var code = -1;
        try
        {
            using var process = new Process { StartInfo = info };
            process.OutputDataReceived += (_, e) => Add(e.Data);
            process.ErrorDataReceived += (_, e) => Add(e.Data);
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();
            code = process.ExitCode;
            logger.LogInformation("extract finished with exit code {ExitCode}", code);
            await catalog.ReloadAsync();
        }
        catch (Exception e)
        {
            Add($"error: {e.Message}");
            logger.LogError(e, "extract failed to run");
        }
        finally
        {
            lock (gate)
            {
                running = false;
                finishedAt = DateTime.UtcNow;
                exitCode = code;
            }
        }
    }

    private void Add(string? line)
    {
        if (line is null) return;
        lock (gate)
            if (output.Count < MaxLines) output.Add(line);
    }
}
