using TaikoLocalServer.Application.Catalog.Yellow;

namespace TaikoLocalServer.Application.Abstractions;

public interface IYellowCatalog : IEraGameDataCatalog
{
    IReadOnlyList<YellowMusicInfoEntry> MusicInfoFileOrder { get; }

    uint SongHashVersion { get; }

    IReadOnlyDictionary<uint, YellowMusicInfoEntry> YellowMusicInfos { get; }

    IReadOnlyList<YellowTaikojukuEntry> TaikojukuFileOrder { get; }

    IReadOnlyDictionary<uint, YellowTaikojukuEntry> Taikojuku { get; }

    YellowItemShopCatalog ItemShopCatalog { get; }

    IReadOnlyDictionary<uint, YellowItemShopEntry> ItemShop { get; }

    IReadOnlyDictionary<uint, EventFolderData> EventFolders { get; }

    IReadOnlyDictionary<uint, YellowTelopEntry> Telops { get; }

    IReadOnlyDictionary<uint, YellowGachaEntry> Gachas { get; }

    IReadOnlyDictionary<uint, YellowTournamentEntry> Tournaments { get; }

    YellowRecommendEntry Recommend { get; }

    IReadOnlyList<MovieData> Movies { get; }

    IReadOnlyList<Costume> GetCostumeList();

    IReadOnlyDictionary<uint, Title> GetTitleDictionary();

    IReadOnlyDictionary<uint, Neiro> GetNeiroDictionary();
}
