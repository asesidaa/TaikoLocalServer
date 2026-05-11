using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Application.Abstractions;

public interface IEraGameDataCatalog
{
    GameEra Era { get; }

    IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos { get; }

    Task InitializeAsync(CancellationToken cancellationToken);
}
