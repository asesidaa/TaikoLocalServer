using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Kimidori;

public static class KimidoriGameDataPaths
{
    public static string GameDataRoot => Path.Combine(PathHelper.GetDataPath(GameEra.Kimidori), "data");

    public static string MusicInfoXml => Path.Combine(GameDataRoot, "musicinfo.xml");

    public static string MusicMedleyInfoXml => Path.Combine(GameDataRoot, "musicmedleyinfo.xml");

    public static string DefMusicBin => Path.Combine(GameDataRoot, "defmusic.bin");

    public static string PresentXml => Path.Combine(GameDataRoot, "present.xml");

    public static string SpecialBaidXml => Path.Combine(GameDataRoot, "spacialbaid.xml");

    public static string TuningBin => Path.Combine(GameDataRoot, "fumen", "tuning.bin");

    public static string MovieDirectory => Path.Combine(GameDataRoot, "movie");
}
