using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Yellow;

public static class YellowGameDataPaths
{
    private const string ConfigDirectory = "ST9100-1";

    public static string GameDataRoot => Path.Combine(PathHelper.GetDataPath(GameEra.Yellow), "data");

    public static string ConfigRoot => Path.Combine(GameDataRoot, "config", ConfigDirectory);

    public static string MusicInfoXml => Path.Combine(ConfigRoot, "musicinfo.xml");

    public static string MusicMedleyInfoXml => Path.Combine(ConfigRoot, "musicmedleyinfo.xml");

    public static string DefMusicBin => Path.Combine(ConfigRoot, "defmusic.bin");

    public static string TuningBin => Path.Combine(GameDataRoot, "fumen", "tuning.bin");

    public static string MovieDirectory => Path.Combine(GameDataRoot, "movie");
}
