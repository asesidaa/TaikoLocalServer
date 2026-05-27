namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

public static class BlueRequiredDataFiles
{
    public static IReadOnlyList<string> GetRequiredPaths()
    {
        return
        [
            BlueGameDataPaths.MusicInfoXml,
            BlueGameDataPaths.MusicMedleyInfoXml,
            BlueGameDataPaths.TuningBin
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
                throw new FileNotFoundException($"Blue required game data file is missing: {path}", path);
            }
        }
    }
}
