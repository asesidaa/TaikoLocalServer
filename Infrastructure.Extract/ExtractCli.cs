using TaikoLocalServer.Infrastructure.Extract.Ac15;
using TaikoLocalServer.Infrastructure.Extract.Nijiiro;

namespace TaikoLocalServer.Infrastructure.Extract;

/// <summary>
/// <c>TaikoLocalServer extract …</c>: copies and converts the game files the server needs into its <c>wwwroot</c>. Exits
/// with 2 when a required file is missing or a step failed, 1 on any other error.
/// </summary>
public static class ExtractCli
{
    private const string Usage = """
        Usage:
          TaikoLocalServer extract --game <Nijiiro install> [--mods] [--out <wwwroot>]
          TaikoLocalServer extract --era <game> --game <folder or .zip> [--out <wwwroot>]
          TaikoLocalServer extract --inputs [--era <game>]

        Without --era, --game is a Nijiiro install (the folder holding Data/x64, or Data/x64 itself); --mods layers its
        LayeredFS Data_mods folder over Data. <game>: green, blue, yellow, red, white, murasaki, kimidori, momoiro; --game
        is then the RPCS3 install (or its USRDIR/data), or a .zip of it. --out defaults to the wwwroot next to the server.
        """;

    public static bool IsExtractCommand(string[] args) => args.Length > 0 && args[0] == "extract";

    /// <summary><paramref name="args"/> without the leading <c>extract</c>.</summary>
    public static int Run(string[] args, string defaultOut, TextWriter output, TextWriter error)
    {
        string? era = null, game = null, outDir = null;
        bool inputs = false, mods = false;
        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--era" when i + 1 < args.Length: era = args[++i]; break;
                case "--game" when i + 1 < args.Length: game = args[++i]; break;
                case "--out" when i + 1 < args.Length: outDir = args[++i]; break;
                case "--inputs": inputs = true; break;
                case "--mods": mods = true; break;
                default:
                    error.WriteLine(Usage);
                    return 1;
            }
        }
        Era parsed = default;
        if (era is not null && !EraInfo.TryParse(era, out parsed))
        {
            error.WriteLine($"Unknown game {era}.\n{Usage}");
            return 1;
        }
        if (inputs)
        {
            output.WriteLine((era is null ? NijiiroExtractor.Inputs() : Ac15Extractor.Inputs(parsed)).ToJsonString());
            return 0;
        }
        if (game is null)
        {
            error.WriteLine(Usage);
            return 1;
        }
        try
        {
            var reports = era is null
                ? NijiiroExtractor.Run(game, outDir ?? defaultOut, mods, output)
                : Ac15Extractor.Run(parsed, game, outDir ?? defaultOut, output);
            foreach (var r in reports)
                r.Print(output);
            return reports.Any(r => r.Missing.Count > 0 || r.Failed.Count > 0) ? 2 : 0;
        }
        catch (Exception e) when (e is IOException or InvalidDataException or UnauthorizedAccessException)
        {
            error.WriteLine($"error: {e.Message}");
            return 1;
        }
    }
}
