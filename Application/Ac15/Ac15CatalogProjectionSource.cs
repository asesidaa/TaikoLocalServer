using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.ServerData;

namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15CatalogProjectionSource(
    uint SongHashVersion,
    IReadOnlyList<uint> SongNoesInFileOrder,
    IReadOnlyDictionary<uint, EventFolderData> EventFolders,
    IReadOnlyDictionary<uint, Ac15TelopEntry> Telops,
    Ac15ItemShopCatalog ItemShopCatalog,
    IReadOnlyList<Ac15TaikojukuEntry> TaikojukuPacks);
