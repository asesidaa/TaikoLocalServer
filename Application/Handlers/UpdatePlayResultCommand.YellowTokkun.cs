using System.Text.Json;

namespace TaikoLocalServer.Application.Handlers;

public partial class UpdatePlayResultCommandHandler
{
    private async ValueTask<uint> HandleYellowTokkun(
        uint baid,
        CommonPlayResultData playResultData,
        CancellationToken cancellationToken)
    {
        var saveData = await context.GetOrCreateYellowSaveDataAsync(baid, cancellationToken);
        if (playResultData.TokkunTutorialFlg is { } tokkunTutorialFlg)
        {
            saveData.TokkunTutorialFlg = tokkunTutorialFlg;
        }

        if (playResultData.TokkunStageData is { } tokkunStageData)
        {
            context.YellowTokkunStageResults.Add(new YellowTokkunStageResult
            {
                Baid = baid,
                PlayDatetime = playResultData.PlayDatetime,
                PlayMode = playResultData.PlayMode,
                BanacoinDatetime = tokkunStageData.BanacoinDatetime,
                TokkunSongCnt = tokkunStageData.TokkunSongCnt,
                TookunSongnoesJson = JsonSerializer.Serialize(tokkunStageData.TookunSongnoes),
                TokkunSpeedchangeCnt = tokkunStageData.TokkunSpeedchangeCnt,
                TokkunAutoplayCnt = tokkunStageData.TokkunAutoplayCnt,
                TokkunJumpCnt = tokkunStageData.TokkunJumpCnt
            });
        }

        await context.SaveChangesAsync(cancellationToken);
        return 1;
    }
}
