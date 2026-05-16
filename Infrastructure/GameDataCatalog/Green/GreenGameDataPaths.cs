using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

internal static class GreenGameDataPaths
{
    private const string ConfigDirectory = "S11100-1";

    public static string GameDataRoot => Path.Combine(PathHelper.GetDataPath(GameEra.Green), "data");

    public static string MusicInfoXml => Path.Combine(GameDataRoot, "config", ConfigDirectory, "musicinfo.xml");

    public static string MusicMedleyInfoXml => Path.Combine(GameDataRoot, "config", ConfigDirectory, "musicmedleyinfo.xml");

    public static string TuningBin => Path.Combine(GameDataRoot, "fumen", "tuning.bin");
}
