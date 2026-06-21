namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Murasaki;

public static class MurasakiRequiredDataFiles
{
    public static IReadOnlyList<string> GetRequiredPaths()
    {
        return
        [
            MurasakiGameDataPaths.MusicInfoXml,
            MurasakiGameDataPaths.MusicMedleyInfoXml,
            MurasakiGameDataPaths.DefMusicBin,
            MurasakiGameDataPaths.PresentXml,
            MurasakiGameDataPaths.SpecialBaidXml,
            MurasakiGameDataPaths.TuningBin
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
                throw new FileNotFoundException($"Murasaki required game data file is missing: {path}", path);
            }
        }
    }
}
