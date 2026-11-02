using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Catalog.Blue;

namespace TaikoLocalServer.Application.Abstractions;

public interface IBlueCatalog : IEraGameDataCatalog
{
    IReadOnlyList<Ac15MusicInfoEntry> MusicInfoFileOrder { get; }

    uint SongHashVersion { get; }

    IReadOnlyDictionary<uint, Ac15MusicInfoEntry> BlueMusicInfos { get; }

    IReadOnlyList<Ac15TaikojukuEntry> TaikojukuFileOrder { get; }

    IReadOnlyDictionary<uint, Ac15TaikojukuEntry> Taikojuku { get; }

    Ac15ItemShopCatalog ItemShopCatalog { get; }

    IReadOnlyDictionary<uint, Ac15ItemShopEntry> ItemShop { get; }

    BlueBattleCatalog BattleCatalog { get; }

    IReadOnlyDictionary<uint, EventFolderData> EventFolders { get; }

    IReadOnlyDictionary<uint, Ac15TelopEntry> Telops { get; }

    IReadOnlyDictionary<uint, Ac15GachaEntry> Gachas { get; }

    IReadOnlyDictionary<uint, Ac15TournamentEntry> Tournaments { get; }

    IReadOnlyList<MovieData> Movies { get; }

    IReadOnlyList<Costume> GetCostumeList();

    IReadOnlyDictionary<uint, Title> GetTitleDictionary();

    IReadOnlyDictionary<uint, Neiro> GetNeiroDictionary();
}
