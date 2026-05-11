namespace TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;

[ApiController]
[Route("/v11r01/chassis/getghostdata.php")]
public class GetGhostDataController : BaseProtocolController<GetGhostDataController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetGhostData([FromBody] GetghostdataRequest request)
    {
        Logger.LogInformation("Green GetGhostData request: {Request}", request.Stringify());
        var common = await Mediator.Send(new GetGhostDataQuery(request.Baid), HttpContext.RequestAborted);
        return Ok(GhostMappers.Map(common));
    }
}
