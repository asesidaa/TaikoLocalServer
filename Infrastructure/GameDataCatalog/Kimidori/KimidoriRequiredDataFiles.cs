namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Kimidori;

public static class KimidoriRequiredDataFiles
{
    public static IReadOnlyList<string> GetRequiredPaths()
    {
        return
        [
            KimidoriGameDataPaths.MusicInfoXml,
            KimidoriGameDataPaths.MusicMedleyInfoXml,
            KimidoriGameDataPaths.DefMusicBin,
            KimidoriGameDataPaths.TuningBin
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
                throw new FileNotFoundException($"Kimidori required game data file is missing: {path}", path);
            }
        }
    }
}
