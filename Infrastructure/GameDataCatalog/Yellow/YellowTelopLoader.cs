using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Yellow;

public sealed class YellowTelopLoader
{
    public const string FileName = "yellow_telop_data.json";

    public Task<IReadOnlyDictionary<uint, Ac15TelopEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        var path = Path.Combine(PathHelper.GetDataPath(GameEra.Yellow), FileName);
        return LoadFromFileAsync(path, cancellationToken);
    }

    public static Task<IReadOnlyDictionary<uint, Ac15TelopEntry>> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
        => Ac15TelopLoader.LoadFromFileAsync(path, cancellationToken);
}
