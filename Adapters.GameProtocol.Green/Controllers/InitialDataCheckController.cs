namespace TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;

[ApiController]
[Route("/v11r01/chassis/initialdatacheck.php")]
public class InitialDataCheckController : BaseProtocolController<InitialDataCheckController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult InitialDataCheck([FromBody] InitialdatacheckRequest request)
    {
        Logger.LogInformation("Green InitialDataCheck request: {Request}", request.Stringify());
        return Ok(new InitialdatacheckResponse
        {
            Result = 1,
            IsDanplay = true,
            IsClose = false,
            IsItemshop = true,
            IsGhostbattleplay = true
        });
    }
}
