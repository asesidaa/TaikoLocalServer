using TaikoLocalServer.Application.Catalog.Blue;

namespace TaikoLocalServer.Application.Abstractions;

public interface IBlueCatalog : IEraGameDataCatalog
{
    IReadOnlyList<BlueMusicInfoEntry> MusicInfoFileOrder { get; }

    uint SongHashVersion { get; }

    IReadOnlyDictionary<uint, BlueMusicInfoEntry> BlueMusicInfos { get; }

    IReadOnlyList<BlueTaikojukuEntry> TaikojukuFileOrder { get; }

    IReadOnlyDictionary<uint, BlueTaikojukuEntry> Taikojuku { get; }

    BlueItemShopCatalog ItemShopCatalog { get; }

    IReadOnlyDictionary<uint, BlueItemShopEntry> ItemShop { get; }

    BlueBattleCatalog BattleCatalog { get; }

    IReadOnlyDictionary<uint, EventFolderData> EventFolders { get; }

    IReadOnlyDictionary<uint, BlueTelopEntry> Telops { get; }

    IReadOnlyDictionary<uint, BlueGachaEntry> Gachas { get; }

    IReadOnlyDictionary<uint, BlueTournamentEntry> Tournaments { get; }

    BlueRecommendEntry Recommend { get; }

    IReadOnlyList<MovieData> Movies { get; }

    IReadOnlyList<Costume> GetCostumeList();

    IReadOnlyDictionary<uint, Title> GetTitleDictionary();

    IReadOnlyDictionary<uint, Neiro> GetNeiroDictionary();
}
