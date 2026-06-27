using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Momoiro;

public static class MomoiroGameDataPaths
{
    public static string GameDataRoot => Path.Combine(PathHelper.GetDataPath(GameEra.Momoiro), "data");

    public static string MusicInfoXml => Path.Combine(GameDataRoot, "musicinfo.xml");

    public static string MusicMedleyInfoXml => Path.Combine(GameDataRoot, "musicmedleyinfo.xml");

    public static string DefMusicBin => Path.Combine(GameDataRoot, "defmusic.bin");

    public static string TuningBin => Path.Combine(GameDataRoot, "fumen", "tuning.bin");

    public static string MovieDirectory => Path.Combine(GameDataRoot, "movie");
}
