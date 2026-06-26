using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Application.Abstractions;

public interface IMomoiroCatalog : IEraGameDataCatalog
{
    IReadOnlyList<Ac15MusicInfoEntry> MusicInfoFileOrder { get; }

    uint SongHashVersion { get; }

    IReadOnlyList<ushort> SongHashTable { get; }

    IReadOnlyDictionary<uint, Ac15MusicInfoEntry> MomoiroMusicInfos { get; }

    IReadOnlyList<Ac15TaikojukuEntry> DaniFileOrder { get; }

    IReadOnlyDictionary<uint, Ac15TelopEntry> Telops { get; }
}
