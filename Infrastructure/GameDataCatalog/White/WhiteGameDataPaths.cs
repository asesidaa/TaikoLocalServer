using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.White;

public static class WhiteGameDataPaths
{
    private const string ConfigDirectory = "ST7100-1";

    public static string GameDataRoot => Path.Combine(PathHelper.GetDataPath(GameEra.White), "data");

    public static string ConfigRoot => Path.Combine(GameDataRoot, "config", ConfigDirectory);

    public static string MusicInfoXml => Path.Combine(ConfigRoot, "musicinfo.xml");

    public static string MusicMedleyInfoXml => Path.Combine(ConfigRoot, "musicmedleyinfo.xml");

    public static string DefMusicBin => Path.Combine(ConfigRoot, "defmusic.bin");

    public static string PresentXml => Path.Combine(ConfigRoot, "present.xml");

    public static string SpecialBaidXml => Path.Combine(ConfigRoot, "spacialbaid.xml");

    public static string TuningBin => Path.Combine(GameDataRoot, "fumen", "tuning.bin");

    public static string MovieDirectory => Path.Combine(GameDataRoot, "movie");
}
