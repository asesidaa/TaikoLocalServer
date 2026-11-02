namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

internal static class GreenRequiredDataFiles
{
    public static IReadOnlyList<string> GetRequiredPaths()
    {
        return
        [
            GreenGameDataPaths.MusicInfoXml,
            GreenGameDataPaths.MusicMedleyInfoXml,
            GreenGameDataPaths.TuningBin
        ];
    }

    public static void ThrowIfMissing()
    {
        foreach (var path in GetRequiredPaths())
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Green required game data file is missing: {path}", path);
            }
        }
    }
}
