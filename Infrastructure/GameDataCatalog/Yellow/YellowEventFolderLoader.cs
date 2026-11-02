using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Yellow;

public sealed class YellowEventFolderLoader
{
    public const string FileName = "yellow_event_folder_data.json";

    public Task<IReadOnlyDictionary<uint, EventFolderData>> LoadAsync(
        IReadOnlySet<uint> catalogSongIds,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(PathHelper.GetDataPath(GameEra.Yellow), FileName);
        return LoadFromFileAsync(path, catalogSongIds, cancellationToken);
    }

    public static Task<IReadOnlyDictionary<uint, EventFolderData>> LoadFromFileAsync(
        string path,
        IReadOnlySet<uint> catalogSongIds,
        CancellationToken cancellationToken)
        => Ac15EventFolderLoader.LoadFromFileAsync(
            path,
            catalogSongIds,
            nameof(GameEra.Yellow),
            cancellationToken);
}
