using TaikoLocalServer.Application.Catalog.Green;

namespace TaikoLocalServer.Application.Abstractions;

public interface IGreenCatalog : IEraGameDataCatalog
{
    IReadOnlyDictionary<uint, GreenMusicInfoEntry> GreenMusicInfos { get; }

    IReadOnlyDictionary<uint, GreenTaikojukuEntry> Taikojuku { get; }

    IReadOnlyDictionary<uint, GreenItemShopEntry> ItemShop { get; }

    IReadOnlyDictionary<uint, GreenEventFolderEntry> EventFolders { get; }

    IReadOnlyDictionary<uint, GreenTelopEntry> Telops { get; }

    IReadOnlyDictionary<uint, GreenGachaEntry> Gachas { get; }

    IReadOnlyDictionary<uint, GreenTournamentEntry> Tournaments { get; }
}
