namespace TaikoLocalServer.Application.Ac15;

public interface IAc15NormalPlayPersistence
{
    ValueTask<bool> UserExistsAsync(uint baid, CancellationToken cancellationToken);

    ValueTask<Ac15SaveSnapshot> GetOrCreateSaveAsync(uint baid, CancellationToken cancellationToken);

    ValueTask AddPlayRowAsync(Ac15PlayRow row, CancellationToken cancellationToken);

    ValueTask UpsertBestAsync(uint baid, Ac15BestRow row, Ac15BestUpdatePolicy policy, CancellationToken cancellationToken);

    ValueTask SetFavoriteAsync(uint baid, uint songNo, bool isFavorite, int maxFavorites, CancellationToken cancellationToken);

    ValueTask UpsertRecentAsync(uint baid, uint songNo, DateTime playTime, CancellationToken cancellationToken);

    ValueTask TrimRecentAsync(uint baid, int maxRecent, CancellationToken cancellationToken);

    ValueTask SaveChangesAsync(CancellationToken cancellationToken);
}
