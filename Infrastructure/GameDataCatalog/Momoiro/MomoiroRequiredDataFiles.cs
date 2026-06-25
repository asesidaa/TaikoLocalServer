namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Momoiro;

public static class MomoiroRequiredDataFiles
{
    public static IReadOnlyList<string> GetRequiredPaths()
    {
        return
        [
            MomoiroGameDataPaths.MusicInfoXml,
            MomoiroGameDataPaths.MusicMedleyInfoXml,
            MomoiroGameDataPaths.DefMusicBin,
            MomoiroGameDataPaths.TuningBin
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
                throw new FileNotFoundException($"Momoiro required game data file is missing: {path}", path);
            }
        }
    }
}
