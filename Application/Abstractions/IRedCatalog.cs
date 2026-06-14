using TaikoLocalServer.Application.Ac15.ChallengeCompe;
using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Application.Abstractions;

public interface IRedCatalog : IEraGameDataCatalog
{
    IReadOnlyList<Ac15MusicInfoEntry> MusicInfoFileOrder { get; }

    uint SongHashVersion { get; }

    IReadOnlyDictionary<uint, Ac15MusicInfoEntry> RedMusicInfos { get; }

    IReadOnlyList<Ac15TaikojukuEntry> TaikojukuFileOrder { get; }

    IReadOnlyDictionary<uint, Ac15TaikojukuEntry> Taikojuku { get; }

    IReadOnlyDictionary<uint, EventFolderData> EventFolders { get; }

    IReadOnlyDictionary<uint, Ac15TelopEntry> Telops { get; }

    Ac15RecommendEntry Recommend { get; }

    IReadOnlyList<MovieData> Movies { get; }

    Ac15ChallengeCompeCatalog ChallengeCompe { get; }

    IReadOnlyList<Costume> GetCostumeList();

    IReadOnlyDictionary<uint, Title> GetTitleDictionary();

    IReadOnlyDictionary<uint, Neiro> GetNeiroDictionary();
}
