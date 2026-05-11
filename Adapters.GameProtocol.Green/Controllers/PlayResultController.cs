namespace TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;

[ApiController]
[Route("/v11r01/chassis/playresult.php")]
public class PlayResultController : BaseProtocolController<PlayResultController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult PlayResult([FromBody] PlayResultRequest request)
    {
        Logger.LogInformation(
            "Green PlayResult request: baid={Baid} chassis={ChassisId}",
            request.BaidConf,
            request.ChassisIdConf);
        return Ok(new PlayResultResponse { Result = 1 });
    }
}
