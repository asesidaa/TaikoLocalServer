using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetDanScoreQueryHandler
{
    private partial async ValueTask<CommonDanScoreDataResponse> HandleGreen(
        GetDanScoreQuery request,
        CancellationToken cancellationToken)
    {
        var limits = Ac15EraProfiles.Green.Limits;
        var knownChallengeLevels = gameDataService.Green().TaikojukuFileOrder
            .Select(pack => pack.ChallengeLevel)
            .Where(id => Ac15DanHelpers.IsKnownDanId(id, limits))
            .ToHashSet();

        var rows = await Ac15DaniReadback.GetScoresAsync(
            GreenDaniTables(),
            request.Baid,
            request.DanIds.ToHashSet(),
            knownChallengeLevels,
            cancellationToken);

        return Ac15DaniReadback.BuildResponse(rows);
    }

    private Ac15DaniTables<DanScoreDatumGreen, DanStageScoreDatumGreen> GreenDaniTables()
        => new(
            context.DanScoreDataGreen,
            context.DanScoreDataGreen.Include(score => score.DanStageScoreData),
            score => score.DanStageScoreData,
            Ac15DaniMapper.ToAc15DaniScore,
            Ac15DaniMapper.ToAc15DaniScoreSummary,
            Ac15DaniMapper.ToGreenDanScoreDatum,
            Ac15DaniMapper.ApplyToGreenDanScoreDatum,
            Ac15DaniMapper.ToGreenDanStageScoreDatum,
            Ac15DaniMapper.ApplyToGreenDanStageScoreDatum);
}
