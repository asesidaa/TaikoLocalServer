namespace TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;

[ApiController]
[Route("/v11r01/chassis/baidcheck.php")]
public class BaidController : BaseProtocolController<BaidController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Baid([FromBody] BAIDRequest request)
    {
        Logger.LogInformation("Green Baid request: {Request}", request.Stringify());
        return Ok(new BAIDResponse
        {
            Result = 1,
            Baid = 1,
            AccessCode = request.AccessCode,
            MydonName = string.Empty,
            IsPublish = true
        });
    }
}
