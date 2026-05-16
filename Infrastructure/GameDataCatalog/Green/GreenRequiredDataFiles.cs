using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

internal static class GreenRequiredDataFiles
{
    public static IReadOnlyList<string> GetRequiredPaths()
    {
        var datatablePath = PathHelper.GetDataTablePath(GameEra.Green);
        return
        [
            Path.Combine(datatablePath, "musicinfo.xml"),
            Path.Combine(datatablePath, "musicmedleyinfo.xml"),
            Path.Combine(datatablePath, "fumen", "tuning.bin")
        ];
    }

    public static void ThrowIfMissing()
    {
        foreach (var path in GetRequiredPaths())
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Green required datatable is missing: {path}", path);
            }
        }
    }
}
