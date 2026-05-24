using TaikoLocalServer.Adapters.GameProtocol.Shared.Wire;

namespace TaikoLocalServer.Adapters.GameProtocol.Shared.Controllers;

[ApiController]
[Route("/v01r00/chassis/startupauth.php")]
public sealed class StartupAuthController : BaseProtocolController<StartupAuthController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult StartupAuth([FromBody] StartupAuthRequest request)
    {
        Logger.LogInformation(
            "StartupAuth request: chassis={ChassisId} shop={ShopId} operation_count={OperationCount}",
            request.ChassisId,
            request.ShopId,
            request.AryOperationInfoes.Count);

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
