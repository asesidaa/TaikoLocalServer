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
        await Ac15TokkunWriter.SaveAsync(
            context,
            new Ac15TokkunTables<BlueRecentSongs>(
                context.BlueRecentSongs,
                AddBlueTokkunHistory),
            new Ac15TokkunWriteRequest(
                baid,
                playResultData,
                saveData.TokkunTutorialFlg,
                value => saveData.TokkunTutorialFlg = value,
                Ac15EraProfiles.Blue.Limits.MaxRecentSongs,
                ParseAc15PlayDatetimeOrNow(playResultData.Metadata.PlayDatetime)),
            cancellationToken);
    }

    private void AddBlueTokkunHistory(
        uint baid,
        Ac15PlayResultEnvelope playResultData,
        Ac15TokkunStageData tokkunStageData)
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
}
