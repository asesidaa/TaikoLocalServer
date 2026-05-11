namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Controllers;

[ApiController]
public class SelfBestController : BaseProtocolController<SelfBestController>
{
    [HttpPost("/v12r08_ww/chassis/selfbest_5nz47auu.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> SelfBest([FromBody] SelfBestRequest request)
    {
        Logger.LogInformation("SelfBest request : {Request}", request.Stringify());

        var commonResponse =
            await Mediator.Send(new GetSelfBestQuery(request.Baid, GameEra.Nijiiro, request.Level, request.ArySongNoes), HttpContext.RequestAborted);
        var response = SelfBestMappers.MapToWW08(commonResponse);

        return Ok(response);
    }
}
