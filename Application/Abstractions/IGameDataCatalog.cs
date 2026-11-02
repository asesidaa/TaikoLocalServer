using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Application.Abstractions;

public interface IGameDataCatalog
{
    IEraGameDataCatalog For(GameEra era);

    Task InitializeAsync(CancellationToken cancellationToken = default);
}
