using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.Persistence;

// The ordered-play questions of ITaikoDbContext, as SQLite window functions (EF Core can't express them).
public partial class TaikoDbContext
{
    public async Task<(int Count, DateTime? Latest)> NewBestTotalsAsync(uint baid, Difficulty? difficulty, CancellationToken cancellationToken)
    {
        var diff = (int?)difficulty ?? 0;
        var row = await Database.SqlQuery<NewBestRow>($"""
            SELECT COUNT(*) AS "Count", MAX(PlayTime) AS "Latest" FROM (
              SELECT PlayTime, Score, MAX(Score) OVER (PARTITION BY SongId, Difficulty ORDER BY PlayTime, Id
                ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING) AS Before
              FROM SongPlayDatum_Nijiiro WHERE Baid = {baid} AND Skipped = 0 AND ({diff} = 0 OR Difficulty = {diff}))
            WHERE Before IS NOT NULL AND Score > Before
            """).FirstAsync(cancellationToken);
        return (row.Count, row.Latest);
    }

    public async Task<Dictionary<uint, int>> CreditCountsAsync(uint? baid, TimeSpan sessionGap, CancellationToken cancellationToken)
    {
        var all = baid is null ? 1 : 0;
        var gapDays = sessionGap.TotalDays;
        var rows = await Database.SqlQuery<CreditRow>($"""
            SELECT Baid, COUNT(*) AS Credits FROM (
              SELECT Baid, SongNumber, PlayTime,
                LAG(SongNumber) OVER (PARTITION BY Baid ORDER BY PlayTime, Id) AS PrevNumber,
                LAG(PlayTime) OVER (PARTITION BY Baid ORDER BY PlayTime, Id) AS PrevTime
              FROM SongPlayDatum_Nijiiro WHERE Skipped = 0 AND ({all} = 1 OR Baid = {baid ?? 0}))
            WHERE PrevNumber IS NULL OR SongNumber = 0 OR SongNumber <= PrevNumber
              OR julianday(PlayTime) - julianday(PrevTime) > {gapDays}
            GROUP BY Baid
            """).ToListAsync(cancellationToken);
        return rows.ToDictionary(r => r.Baid, r => r.Credits);
    }

    private sealed class NewBestRow
    {
        public int Count { get; set; }
        public DateTime? Latest { get; set; }
    }

    private sealed class CreditRow
    {
        public uint Baid { get; set; }
        public int Credits { get; set; }
    }
}
