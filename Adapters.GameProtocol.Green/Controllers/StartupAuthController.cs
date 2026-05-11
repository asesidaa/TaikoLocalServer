namespace TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;

[ApiController]
[Route("/v11r01/chassis/startupauth.php")]
public class StartupAuthController : BaseProtocolController<StartupAuthController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult StartupAuth([FromBody] StartupAuthRequest request)
    {
        Logger.LogInformation("Green StartupAuth request: {Request}", request.Stringify());
        var response = new StartupAuthResponse { Result = 1 };
        response.AryOperationInfoes.AddRange(request.AryOperationInfoes.Select(input =>
            new StartupAuthResponse.OperationData
            {
                KeyData = input.KeyData,
                ValueData = input.ValueData
            }));
        return Ok(response);
    }
}
