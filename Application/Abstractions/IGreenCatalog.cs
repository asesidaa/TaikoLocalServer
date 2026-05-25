using TaikoLocalServer.Application.Catalog.Green;

namespace TaikoLocalServer.Application.Abstractions;

public interface IGreenCatalog : IEraGameDataCatalog
{
    IReadOnlyList<GreenMusicInfoEntry> MusicInfoFileOrder { get; }

    uint SongHashVersion { get; }

    IReadOnlyDictionary<uint, GreenMusicInfoEntry> GreenMusicInfos { get; }

    IReadOnlyList<GreenTaikojukuEntry> TaikojukuFileOrder { get; }

    IReadOnlyDictionary<uint, GreenTaikojukuEntry> Taikojuku { get; }

    IReadOnlyDictionary<uint, GreenItemShopEntry> ItemShop { get; }

    IReadOnlyDictionary<uint, GreenEventFolderEntry> EventFolders { get; }

    IReadOnlyDictionary<uint, GreenTelopEntry> Telops { get; }

    IReadOnlyDictionary<uint, GreenGachaEntry> Gachas { get; }

    IReadOnlyDictionary<uint, GreenTournamentEntry> Tournaments { get; }

    GreenRecommendEntry Recommend { get; }

    IReadOnlyList<MovieData> Movies { get; }

    IReadOnlyList<Costume> GetCostumeList();

    IReadOnlyDictionary<uint, Title> GetTitleDictionary();

    IReadOnlyDictionary<uint, Neiro> GetNeiroDictionary();
}
