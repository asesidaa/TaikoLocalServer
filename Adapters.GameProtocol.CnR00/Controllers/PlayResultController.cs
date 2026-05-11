namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Controllers;

[ApiController]
public class PlayResultController : BaseProtocolController<PlayResultController>
{
    [HttpPost("/v12r00_cn/chassis/playresult.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> UploadPlayResultCN00([FromBody] PlayResultRequest request)
    {
        Logger.LogInformation("PlayResultCN00 request : {Request}", request.Stringify());
        var decompressed = GZipBytesUtil.DecompressGZipBytes(request.PlayresultData);
        var playResultData =
            Serializer.Deserialize<PlayResultDataRequest>(new ReadOnlySpan<byte>(decompressed));
        Logger.LogInformation("Play result data CN00 {Data}", playResultData.Stringify());

        var commonRequest = PlayResultMappers.Map(playResultData);
        var commonResponse = await Mediator.Send(new UpdatePlayResultCommand((uint) request.BaidConf, GameEra.Nijiiro, commonRequest), HttpContext.RequestAborted);
        var response = new PlayResultResponse
        {
            Result = commonResponse
        };
        return Ok(response);
    }
}
