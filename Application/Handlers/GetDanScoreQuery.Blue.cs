using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetDanScoreQueryHandler
{
    private partial async ValueTask<CommonDanScoreDataResponse> HandleBlue(
        GetDanScoreQuery request,
        CancellationToken cancellationToken)
    {
        var limits = Ac15EraProfiles.Blue.Limits;
        var knownChallengeLevels = gameDataService.Blue().TaikojukuFileOrder
            .Select(pack => pack.ChallengeLevel)
            .Where(id => Ac15DanHelpers.IsKnownDanId(id, limits))
            .ToHashSet();

        var rows = await Ac15DaniReadback.GetScoresAsync(
            BlueDaniTables(),
            request.Baid,
            request.DanIds.ToHashSet(),
            knownChallengeLevels,
            cancellationToken);

        return Ac15DaniReadback.BuildResponse(rows);
    }

    private Ac15DaniTables<DanScoreDatumBlue, DanStageScoreDatumBlue> BlueDaniTables()
        => new(
            context.DanScoreDataBlue,
            context.DanScoreDataBlue.Include(score => score.DanStageScoreData),
            score => score.DanStageScoreData,
            Ac15DaniMapper.ToAc15DaniScore,
            Ac15DaniMapper.ToAc15DaniScoreSummary,
            Ac15DaniMapper.ToBlueDanScoreDatum,
            Ac15DaniMapper.ApplyToBlueDanScoreDatum,
            Ac15DaniMapper.ToBlueDanStageScoreDatum,
            Ac15DaniMapper.ApplyToBlueDanStageScoreDatum);
}
