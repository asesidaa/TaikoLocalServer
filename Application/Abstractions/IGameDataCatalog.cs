using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Application.Abstractions;

public interface IGameDataCatalog
{
    IEraGameDataCatalog For(GameEra era);

    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>How each enabled game's files last loaded (start-up or <see cref="ReloadAsync"/>).</summary>
    IReadOnlyDictionary<GameEra, EraLoadResult> LoadResults { get; }

    /// <summary>Loads one game's files again, as at start-up (Server Management › Status › Check again).</summary>
    Task<EraLoadResult> ReloadAsync(GameEra era, CancellationToken cancellationToken = default);
}

/// <summary>One game's last load: <see cref="Error"/> is null when it loaded.</summary>
public sealed record EraLoadResult(GameEra Era, DateTime At, string? Error);
