namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Controllers;

[ApiController]
public class PlayResultController : BaseProtocolController<PlayResultController>
{
    [HttpPost("/v12r08_ww/chassis/playresult_r3ky4a4z.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> UploadPlayResult([FromBody] PlayResultRequest request)
    {
        Logger.LogInformation("PlayResult request : {@Request}", request);

        var truncated = request.PlayresultData.Skip(32).ToArray();
        var decompressed = GZipBytesUtil.DecompressGZipBytes(truncated);
        var playResultData = Serializer.Deserialize<PlayResultDataRequest>(new ReadOnlySpan<byte>(decompressed));
        Logger.LogInformation("Play result data {@Data}", playResultData);

        var commonRequest = PlayResultMappers.Map(playResultData);
        var commonResponse = await Mediator.Send(new UpdatePlayResultCommand(request.BaidConf, GameEra.Nijiiro, commonRequest), HttpContext.RequestAborted);
        var response = new PlayResultResponse
        {
            Result = commonResponse
        };
        return Ok(response);
    }
}
