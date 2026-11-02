using System.Text.Json;
using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class UpdatePlayResultCommandHandler
{
    private async ValueTask<uint> HandleWhiteTokkun(
        uint baid,
        Ac15PlayResultEnvelope playResultData,
        CancellationToken cancellationToken)
    {
        var saveData = await context.GetOrCreateWhiteSaveDataAsync(baid, cancellationToken);
        await Ac15TokkunWriter.SaveAsync(
            context,
            new Ac15TokkunTables<WhiteRecentSongs>(
                context.WhiteRecentSongs,
                AddWhiteTokkunHistory),
            new Ac15TokkunWriteRequest(
                baid,
                playResultData,
                saveData.TokkunTutorialFlg,
                value => saveData.TokkunTutorialFlg = value,
                Ac15EraProfiles.White.Limits.MaxRecentSongs,
                ParseAc15PlayDatetimeOrNow(playResultData.Metadata.PlayDatetime)),
            cancellationToken);
        return 1;
    }

    private void AddWhiteTokkunHistory(
        uint baid,
        Ac15PlayResultEnvelope playResultData,
        Ac15TokkunStageData tokkunStageData)
    {
        context.WhiteTokkunStageResults.Add(new WhiteTokkunStageResult
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
