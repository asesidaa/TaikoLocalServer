using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Application.Abstractions;

public interface IKimidoriCatalog : IEraGameDataCatalog
{
    IReadOnlyList<Ac15MusicInfoEntry> MusicInfoFileOrder { get; }

    uint SongHashVersion { get; }

    IReadOnlyList<ushort> SongHashTable { get; }

    IReadOnlyDictionary<uint, Ac15MusicInfoEntry> KimidoriMusicInfos { get; }

    IReadOnlyList<Ac15TaikojukuEntry> DaniFileOrder { get; }

    IReadOnlyList<Ac15TaikojukuEntry> TaikojukuFileOrder { get; }

    IReadOnlyDictionary<uint, EventFolderData> EventFolders { get; }

    IReadOnlyDictionary<uint, Ac15TelopEntry> Telops { get; }

    IReadOnlyList<MovieData> Movies { get; }

    IReadOnlyList<Ac15PresentItem> Presents { get; }

    IReadOnlyList<Ac15SpecialBaidEntry> SpecialBaids { get; }

    IReadOnlyList<Costume> GetCostumeList();

    IReadOnlyDictionary<uint, Title> GetTitleDictionary();

    IReadOnlyDictionary<uint, Neiro> GetNeiroDictionary();
}
