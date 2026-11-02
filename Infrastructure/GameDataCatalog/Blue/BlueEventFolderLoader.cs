using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

public sealed class BlueEventFolderLoader
{
    public const string FileName = "blue_event_folder_data.json";

    public Task<IReadOnlyDictionary<uint, EventFolderData>> LoadAsync(
        IReadOnlySet<uint> catalogSongIds,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(PathHelper.GetDataPath(GameEra.Blue), FileName);
        return Ac15EventFolderLoader.LoadFromFileAsync(
            path,
            catalogSongIds,
            nameof(GameEra.Blue),
            cancellationToken);
    }
}
