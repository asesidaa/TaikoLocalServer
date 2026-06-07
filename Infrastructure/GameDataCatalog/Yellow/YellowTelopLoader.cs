using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Catalog.Yellow;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Yellow;

public sealed class YellowTelopLoader
{
    public const string FileName = "yellow_telop_data.json";

    public Task<IReadOnlyDictionary<uint, YellowTelopEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        var path = Path.Combine(PathHelper.GetDataPath(GameEra.Yellow), FileName);
        return LoadFromFileAsync(path, cancellationToken);
    }

    public static async Task<IReadOnlyDictionary<uint, YellowTelopEntry>> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        var telops = await Ac15TelopLoader.LoadFromFileAsync(path, cancellationToken);
        return telops.ToDictionary(
            pair => pair.Key,
            pair => Map(pair.Value));
    }

    private static YellowTelopEntry Map(Ac15TelopEntry entry) => new()
    {
        TelopId = entry.TelopId,
        VerupNo = entry.VerupNo,
        StartDatetime = entry.StartDatetime,
        EndDatetime = entry.EndDatetime,
        Message = entry.Message
    };
}
