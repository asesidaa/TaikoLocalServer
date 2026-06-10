namespace TaikoLocalServer.Application.Ac15;

public interface IAc15DaniPersistence
{
    IReadOnlySet<uint> KnownChallengeLevels { get; }

    ValueTask<Ac15DaniScore?> GetScoreAsync(
        Ac15DaniScoreKey key,
        CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<Ac15DaniScore>> GetScoresAsync(
        uint baid,
        IReadOnlySet<uint> requestedDanIds,
        CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<Ac15DaniScoreSummary>> GetScoreSummariesAsync(
        uint baid,
        CancellationToken cancellationToken);

    ValueTask UpsertScoreAsync(
        Ac15DaniScore score,
        CancellationToken cancellationToken);

    ValueTask SaveChangesAsync(CancellationToken cancellationToken);
}
