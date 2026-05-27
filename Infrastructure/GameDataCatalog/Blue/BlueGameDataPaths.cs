using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

public static class BlueGameDataPaths
{
    private const string ConfigDirectory = "S10100-1";

    public static string GameDataRoot => Path.Combine(PathHelper.GetDataPath(GameEra.Blue), "data");

    public static string ConfigRoot => Path.Combine(GameDataRoot, "config", ConfigDirectory);

    public static string MusicInfoXml => Path.Combine(ConfigRoot, "musicinfo.xml");

    public static string MusicMedleyInfoXml => Path.Combine(ConfigRoot, "musicmedleyinfo.xml");

    public static string TuningBin => Path.Combine(GameDataRoot, "fumen", "tuning.bin");

    public static string MovieDirectory => Path.Combine(GameDataRoot, "movie");
}
