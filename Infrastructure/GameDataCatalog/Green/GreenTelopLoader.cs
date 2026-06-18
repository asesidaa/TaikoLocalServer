using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenTelopLoader
{
    public Task<IReadOnlyDictionary<uint, Ac15TelopEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        var path = Path.Combine(PathHelper.GetDataPath(GameEra.Green), "telop_data.json");
        return LoadFromFileAsync(path, cancellationToken);
    }

    public static Task<IReadOnlyDictionary<uint, Ac15TelopEntry>> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
        => Ac15TelopLoader.LoadFromFileAsync(path, cancellationToken);
}
