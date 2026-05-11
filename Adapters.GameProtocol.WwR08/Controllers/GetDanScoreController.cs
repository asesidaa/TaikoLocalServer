namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Controllers;

[ApiController]
public class GetDanScoreController : BaseProtocolController<GetDanScoreController>
{
    [HttpPost("/v12r08_ww/chassis/getdanscore_frqhg7q6.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetDanScore([FromBody] GetDanScoreRequest request)
    {
        Logger.LogInformation("GetDanScore request : {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new GetDanScoreQuery(request.Baid, GameEra.Nijiiro, request.Type, request.DanIds), HttpContext.RequestAborted);
        var response = DanScoreMappers.MapToWW08(commonResponse);

        return Ok(response);
    }
}
