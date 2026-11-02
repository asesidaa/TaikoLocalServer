using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

public sealed class BlueTitleLoader
{
    public const string FileName = "blue_title_data.json";

    public async Task<IReadOnlyDictionary<uint, Title>> LoadAsync(CancellationToken cancellationToken)
    {
        var items = await Ac15CustomizationCatalogLoader.LoadListAsync<Title>(
            Path.Combine(PathHelper.GetDataPath(GameEra.Blue), FileName),
            cancellationToken);
        return items
            .GroupBy(title => title.TitleId)
            .ToDictionary(group => group.Key, group => group.First());
    }
}
