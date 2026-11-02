using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

public sealed class BlueTelopLoader
{
    public const string FileName = "blue_telop_data.json";

    public Task<IReadOnlyDictionary<uint, Ac15TelopEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        var path = Path.Combine(PathHelper.GetDataPath(GameEra.Blue), FileName);
        return Ac15TelopLoader.LoadFromFileAsync(path, cancellationToken);
    }
}
