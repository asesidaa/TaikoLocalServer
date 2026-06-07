namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Yellow;

public static class YellowRequiredDataFiles
{
    public static IReadOnlyList<string> GetRequiredPaths()
    {
        return
        [
            YellowGameDataPaths.MusicInfoXml,
            YellowGameDataPaths.MusicMedleyInfoXml,
            YellowGameDataPaths.DefMusicBin,
            YellowGameDataPaths.TuningBin
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
                throw new FileNotFoundException($"Yellow required game data file is missing: {path}", path);
            }
        }
    }
}
