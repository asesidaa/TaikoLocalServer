using Garm;

namespace TaikoLocalServer.Adapters.AllnetMucha.Controllers.Garmc;

[Route("/v1/s12-jp-dev/garm.SystemBoard/RegisterSystemBoardBilling")]
[ApiController]
public class RegisterSystemBoardBillingController : BaseProtocolController<RegisterSystemBoardBillingController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<RegisterSystemBoardBillingResponse> RegisterSystemBoardBilling()
    {
        HttpContext.Request.EnableBuffering();
        var body = await HttpContext.Request.BodyReader.ReadAsync();
        var request = Serializer.Deserialize<RegisterSystemBoardBillingRequest>(body.Buffer);
        Logger.LogInformation("RegisterSystemBoardBilling request: {@Request}", request);
        var response = new RegisterSystemBoardBillingResponse
        {
            TooMany = false
        };
        Response.Headers.Append("x-drpc-code", "0");
        return response;
    }
}