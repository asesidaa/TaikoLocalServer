using System.Text.Json;

namespace TaikoLocalServer.Application.Handlers;

public partial class UpdatePlayResultCommandHandler
{
    private async ValueTask<uint> HandleBlueTokkun(
        uint baid,
        CommonPlayResultData playResultData,
        CancellationToken cancellationToken)
    {
        await SaveBlueTokkun(baid, playResultData, cancellationToken);
        return 1;
    }

    private async ValueTask SaveBlueTokkun(
        uint baid,
        CommonPlayResultData playResultData,
        CancellationToken cancellationToken)
    {
        var saveData = await context.GetOrCreateBlueSaveDataAsync(baid, cancellationToken);
        if (playResultData.TokkunTutorialFlg is { } tokkunTutorialFlg)
        {
            saveData.TokkunTutorialFlg = tokkunTutorialFlg;
        }

        if (playResultData.TokkunStageData is { } tokkunStageData)
        {
            context.BlueTokkunStageResults.Add(new BlueTokkunStageResult
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
    }
}
