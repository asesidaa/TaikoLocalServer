using TaikoLocalServer.Common.Utils;
using TaikoLocalServer.Mappers;

namespace TaikoLocalServer.Controllers.Game;

[ApiController]
public class PlayResultController : BaseController<PlayResultController>
{
    [HttpPost("/v12r08_ww/chassis/playresult_r3ky4a4z.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> UploadPlayResult([FromBody] PlayResultRequest request)
    {
        Logger.LogInformation("PlayResult request : {Request}", request.Stringify());

        var truncated = request.PlayresultData.Skip(32).ToArray();
        var decompressed = GZipBytesUtil.DecompressGZipBytes(truncated);
        var playResultData = Serializer.Deserialize<PlayResultDataRequest>(new ReadOnlySpan<byte>(decompressed));
        Logger.LogInformation("Play result data {Data}", playResultData.Stringify());

        var commonRequest = PlayResultMappers.Map(playResultData);
        var commonResponse = await Mediator.Send(new UpdatePlayResultCommand(request.BaidConf, commonRequest), HttpContext.RequestAborted);
        var response = new PlayResultResponse
        {
            Result = commonResponse
        };
        return Ok(response);
    }

    [HttpPost("/v12r00_cn/chassis/playresult.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> UploadPlayResultCN00([FromBody] Models.CN00.PlayResultRequest request)
    {
        Logger.LogInformation("PlayResultCN00 request : {Request}", request.Stringify());
        var decompressed = GZipBytesUtil.DecompressGZipBytes(request.PlayresultData);
        var playResultData =
            Serializer.Deserialize<Models.CN00.PlayResultDataRequest>(new ReadOnlySpan<byte>(decompressed));
        Logger.LogInformation("Play result data CN00 {Data}", playResultData.Stringify());

        var commonRequest = PlayResultMappers.Map(playResultData);
        var commonResponse = await Mediator.Send(new UpdatePlayResultCommand((uint) request.BaidConf, commonRequest), HttpContext.RequestAborted);
        var response = new Models.CN00.PlayResultResponse
        {
            Result = commonResponse
        };
        return Ok(response);
    }
}