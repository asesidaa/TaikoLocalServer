using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Catalog.Green;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenTelopLoader
{
    public Task<IReadOnlyDictionary<uint, GreenTelopEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        var path = Path.Combine(PathHelper.GetDataPath(GameEra.Green), "telop_data.json");
        return LoadFromFileAsync(path, cancellationToken);
    }

    public static async Task<IReadOnlyDictionary<uint, GreenTelopEntry>> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        var telops = await Ac15TelopLoader.LoadFromFileAsync(path, cancellationToken);
        return telops.ToDictionary(
            pair => pair.Key,
            pair => Map(pair.Value));
    }

    private static GreenTelopEntry Map(Ac15TelopEntry entry) => new()
    {
        TelopId = entry.TelopId,
        VerupNo = entry.VerupNo,
        StartDatetime = entry.StartDatetime,
        EndDatetime = entry.EndDatetime,
        Message = entry.Message
    };
}
