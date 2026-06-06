using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15CatalogSnapshot(
    uint SongHashVersion,
    IReadOnlyList<uint> SongNoesInFileOrder,
    IReadOnlyDictionary<uint, EventFolderData> EventFolders,
    IReadOnlyDictionary<uint, Ac15TelopEntry> Telops,
    uint RecommendSong,
    IReadOnlyList<uint> RecommendBestSongs,
    Ac15ItemShopCatalog ItemShopCatalog,
    IReadOnlyList<Ac15TaikojukuEntry> TaikojukuPacks);
