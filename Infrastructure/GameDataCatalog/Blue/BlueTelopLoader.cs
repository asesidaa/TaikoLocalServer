using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Catalog.Blue;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

public sealed class BlueTelopLoader
{
    public const string FileName = "blue_telop_data.json";

    public async Task<IReadOnlyDictionary<uint, BlueTelopEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        var path = Path.Combine(PathHelper.GetDataPath(GameEra.Blue), FileName);
        var telops = await Ac15TelopLoader.LoadFromFileAsync(path, cancellationToken);
        return telops.ToDictionary(
            pair => pair.Key,
            pair => Map(pair.Value));
    }

    private static BlueTelopEntry Map(Ac15TelopEntry entry) => new()
    {
        TelopId = entry.TelopId,
        VerupNo = entry.VerupNo,
        StartDatetime = entry.StartDatetime,
        EndDatetime = entry.EndDatetime,
        Message = entry.Message
    };
}
