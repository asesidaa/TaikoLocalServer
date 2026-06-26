using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class UpdatePlayResultCommandHandler
{
    private partial async ValueTask<uint> HandleMomoiro(
        UpdateAc15PlayResultCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Baid == 0)
        {
            return 1;
        }

        var user = await context.UserData.FindAsync([request.Baid], cancellationToken);
        if (user is null)
        {
            logger.LogWarning("Game uploading a non existing Momoiro user with baid {Baid}", request.Baid);
            return 1;
        }

        var playResultData = request.PlayResultData;
        var normal = playResultData.Normal;
        IReadOnlyList<Ac15StageResult> stages = normal?.Stages ?? [];

        var validStages = Ac15NormalStageFilter.Filter(
            request.Baid,
            stages,
            Ac15EraProfiles.Momoiro.Limits,
            Ac15NormalStagePolicies.Standard,
            logger);
        if (validStages.Count == 0)
        {
            logger.LogWarning("Skipping Momoiro playresult with no valid normal stages for baid {Baid}", request.Baid);
            return 1;
        }

        _ = await context.GetOrCreateMomoiroSaveDataAsync(request.Baid, cancellationToken);
        _ = gameDataService.Momoiro();
        _ = ParseAc15PlayDatetimeOrNow(playResultData.Metadata.PlayDatetime);

        return 1;
    }
}
