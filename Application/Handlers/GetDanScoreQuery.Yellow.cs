using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetDanScoreQueryHandler
{
    private partial async ValueTask<CommonDanScoreDataResponse> HandleYellow(
        GetDanScoreQuery request,
        CancellationToken cancellationToken)
    {
        var limits = Ac15EraProfiles.Yellow.Limits;
        var knownChallengeLevels = gameDataService.Yellow().TaikojukuFileOrder
            .Select(pack => pack.ChallengeLevel)
            .Where(id => Ac15DanHelpers.IsKnownDanId(id, limits))
            .ToHashSet();

        var rows = await Ac15DaniReadback.GetScoresAsync(
            YellowDaniTables(),
            request.Baid,
            request.DanIds.ToHashSet(),
            knownChallengeLevels,
            cancellationToken);

        return Ac15DaniReadback.BuildResponse(rows);
    }

    private Ac15DaniTables<DanScoreDatumYellow, DanStageScoreDatumYellow> YellowDaniTables()
        => new(
            context.DanScoreDataYellow,
            context.DanScoreDataYellow.Include(score => score.DanStageScoreData),
            score => score.DanStageScoreData,
            Ac15DaniMapper.ToAc15DaniScore,
            Ac15DaniMapper.ToAc15DaniScoreSummary,
            Ac15DaniMapper.ToYellowDanScoreDatum,
            Ac15DaniMapper.ApplyToYellowDanScoreDatum,
            Ac15DaniMapper.ToYellowDanStageScoreDatum,
            Ac15DaniMapper.ApplyToYellowDanStageScoreDatum);
}
