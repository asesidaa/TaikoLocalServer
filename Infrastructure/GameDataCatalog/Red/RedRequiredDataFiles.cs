namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Red;

public static class RedRequiredDataFiles
{
    public static IReadOnlyList<string> GetRequiredPaths()
    {
        return
        [
            RedGameDataPaths.MusicInfoXml,
            RedGameDataPaths.MusicMedleyInfoXml,
            RedGameDataPaths.DefMusicBin,
            RedGameDataPaths.TuningBin
        ];
    }

    public static void ThrowIfMissing()
        => ThrowIfMissing(GetRequiredPaths());

    public static void ThrowIfMissing(IEnumerable<string> paths)
    {
        foreach (var path in paths)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Red required game data file is missing: {path}", path);
            }
        }
    }
}
