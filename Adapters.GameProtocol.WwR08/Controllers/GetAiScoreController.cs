namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Controllers;

[ApiController]
public class GetAiScoreController : BaseProtocolController<GetAiScoreController>
{
    [HttpPost("/v12r08_ww/chassis/getaiscore_lp38po4w.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetAiScore([FromBody] GetAiScoreRequest request)
    {
        Logger.LogInformation("GetAiScore request : {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new GetAiScoreQuery(request.Baid, request.SongNo, request.Level), HttpContext.RequestAborted);
        var response = AiScoreMappers.MapToWW08(commonResponse);

        return Ok(response);
    }
}
