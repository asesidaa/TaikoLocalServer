using taiko.vsinterface;

namespace TaikoLocalServer.Controllers.Game;

[ApiController]
[Route("/v01r00/chassis/startupauth.php")]
public class StartupAuthController : BaseController<StartupAuthController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult StartupAuth([FromBody] StartupAuthRequest request)
    {
        Logger.LogInformation("StartupAuth request: {Request}", request.Stringify());
        var response = new StartupAuthResponse
        {
            Result = 1
        };
        var info = request.AryOperationInfoes.ConvertAll(input =>
            new StartupAuthResponse.OperationData
            {
                KeyData = input.KeyData,
                ValueData = input.ValueData
            });
        response.AryOperationInfoes.AddRange(info);

        return Ok(response);
    }
}