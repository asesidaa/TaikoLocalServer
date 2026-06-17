using System.Text.Json;
using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class UpdatePlayResultCommandHandler
{
    private async ValueTask<uint> HandleBlueTokkun(
        uint baid,
        Ac15PlayResultEnvelope playResultData,
        CancellationToken cancellationToken)
    {
        await SaveBlueTokkun(baid, playResultData, cancellationToken);
        return 1;
    }

    private async ValueTask SaveBlueTokkun(
        uint baid,
        Ac15PlayResultEnvelope playResultData,
        CancellationToken cancellationToken)
    {
        var saveData = await context.GetOrCreateBlueSaveDataAsync(baid, cancellationToken);
        saveData.TokkunTutorialFlg = Ac15CommonProfileMutation.PreserveTutorialFlag(
            saveData.TokkunTutorialFlg,
            playResultData.Tokkun?.TutorialFlg);

        if (playResultData.Tokkun?.StageData is { } tokkunStageData)
        {
            context.BlueTokkunStageResults.Add(new BlueTokkunStageResult
            {
                Baid = baid,
                PlayDatetime = playResultData.Metadata.PlayDatetime,
                PlayMode = playResultData.Metadata.PlayMode,
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
