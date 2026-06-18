namespace TaikoLocalServer.Infrastructure.GameDataCatalog.White;

public static class WhiteRequiredDataFiles
{
    public static IReadOnlyList<string> GetRequiredPaths()
    {
        return
        [
            WhiteGameDataPaths.MusicInfoXml,
            WhiteGameDataPaths.MusicMedleyInfoXml,
            WhiteGameDataPaths.DefMusicBin,
            WhiteGameDataPaths.TuningBin
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
                throw new FileNotFoundException($"White required game data file is missing: {path}", path);
            }
        }
    }
}
