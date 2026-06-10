namespace TaikoLocalServer.Application.Ac15;

public sealed class YellowAc15DaniAdapter(ITaikoDbContext context, IEnumerable<uint> knownChallengeLevels)
    : IAc15DaniPersistence
{
    public IReadOnlySet<uint> KnownChallengeLevels { get; } = knownChallengeLevels.ToHashSet();

    public async ValueTask<Ac15DaniScore?> GetScoreAsync(
        Ac15DaniScoreKey key,
        CancellationToken cancellationToken)
    {
        var row = await context.DanScoreDataYellow
            .Include(score => score.DanStageScoreData)
            .SingleOrDefaultAsync(
                score => score.Baid == key.Baid
                         && score.DanId == key.DanId
                         && score.IsExtra == key.IsExtra,
                cancellationToken);

        return row is null ? null : Map(row);
    }

    public async ValueTask<IReadOnlyList<Ac15DaniScore>> GetScoresAsync(
        uint baid,
        IReadOnlySet<uint> requestedDanIds,
        CancellationToken cancellationToken)
    {
        var validRequestedIds = requestedDanIds
            .Where(KnownChallengeLevels.Contains)
            .ToHashSet();

        var rows = await context.DanScoreDataYellow
            .Where(row => row.Baid == baid && validRequestedIds.Contains(row.DanId))
            .Include(row => row.DanStageScoreData)
            .ToListAsync(cancellationToken);

        return rows
            .OrderBy(row => row.DanId)
            .Select(Map)
            .ToArray();
    }

    public async ValueTask<IReadOnlyList<Ac15DaniScoreSummary>> GetScoreSummariesAsync(
        uint baid,
        CancellationToken cancellationToken)
    {
        var rows = await context.DanScoreDataYellow
            .Where(row => row.Baid == baid)
            .ToListAsync(cancellationToken);

        return rows
            .Select(row => new Ac15DaniScoreSummary(row.DanId, row.IsExtra, row.ClearGrade))
            .ToArray();
    }

    public async ValueTask UpsertScoreAsync(
        Ac15DaniScore score,
        CancellationToken cancellationToken)
    {
        var row = await context.DanScoreDataYellow
            .Include(existing => existing.DanStageScoreData)
            .SingleOrDefaultAsync(
                existing => existing.Baid == score.Baid
                            && existing.DanId == score.DanId
                            && existing.IsExtra == score.IsExtra,
                cancellationToken);

        if (row is null)
        {
            row = new DanScoreDatumYellow
            {
                Baid = score.Baid,
                DanId = score.DanId,
                IsExtra = score.IsExtra
            };
            context.DanScoreDataYellow.Add(row);
        }

        row.MedleyUniqueId = score.MedleyUniqueId;
        row.ArrivalSongCount = score.ArrivalSongCount;
        row.SoulGaugeTotal = score.SoulGaugeTotal;
        row.ComboCountTotal = score.ComboCountTotal;
        row.ClearGrade = score.ClearGrade;

        foreach (var stage in score.Stages)
        {
            var stageRow = row.DanStageScoreData.FirstOrDefault(existing => existing.StageIndex == stage.StageIndex);
            if (stageRow is null)
            {
                stageRow = new DanStageScoreDatumYellow
                {
                    Baid = score.Baid,
                    DanId = score.DanId,
                    IsExtra = score.IsExtra,
                    StageIndex = stage.StageIndex
                };
                row.DanStageScoreData.Add(stageRow);
            }

            stageRow.SongNumber = stage.SongNumber;
            stageRow.PlayScore = stage.PlayScore;
            stageRow.GoodCount = stage.GoodCount;
            stageRow.OkCount = stage.OkCount;
            stageRow.BadCount = stage.BadCount;
            stageRow.DrumrollCount = stage.DrumrollCount;
            stageRow.TotalHitCount = stage.TotalHitCount;
            stageRow.ComboCount = stage.ComboCount;
            stageRow.HighScore = stage.HighScore;
        }
    }

    public ValueTask SaveChangesAsync(CancellationToken cancellationToken)
        => new(context.SaveChangesAsync(cancellationToken));

    private static Ac15DaniScore Map(DanScoreDatumYellow row)
        => new(
            row.Baid,
            row.DanId,
            row.IsExtra,
            row.MedleyUniqueId,
            row.ArrivalSongCount,
            row.SoulGaugeTotal,
            row.ComboCountTotal,
            row.ClearGrade,
            row.DanStageScoreData
                .OrderBy(stage => stage.StageIndex)
                .Select(MapStage)
                .ToArray());

    private static Ac15DaniStageScore MapStage(DanStageScoreDatumYellow stage)
        => new(
            stage.StageIndex,
            stage.SongNumber,
            stage.PlayScore,
            stage.GoodCount,
            stage.OkCount,
            stage.BadCount,
            stage.DrumrollCount,
            stage.TotalHitCount,
            stage.ComboCount,
            stage.HighScore);
}
