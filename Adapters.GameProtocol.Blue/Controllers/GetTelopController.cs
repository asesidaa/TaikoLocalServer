namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/gettelop.php")]
public class GetTelopController : BaseProtocolController<GetTelopController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetTelop([FromBody] GettelopRequest request)
    {
        Logger.LogInformation("Blue GetTelop request: {Request}", request.Stringify());
        return Ok(new GettelopResponse { Result = 1 });
    }
}
