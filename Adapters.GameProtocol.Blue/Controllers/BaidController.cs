namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/baidcheck.php")]
public class BaidController : BaseProtocolController<BaidController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Baid([FromBody] BAIDRequest request)
    {
        Logger.LogInformation("Blue BAID request: {Request}", request.Stringify());
        return Ok(new BAIDResponse
        {
            Result = 1,
            PlayerType = 1,
            AccessCode = request.AccessCode,
            IsPublish = true
        });
    }
}
