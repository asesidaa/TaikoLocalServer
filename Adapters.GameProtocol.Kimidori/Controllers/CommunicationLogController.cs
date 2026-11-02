namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Controllers;

[ApiController]
public sealed class CommunicationLogController : BaseProtocolController<CommunicationLogController>
{
    [HttpPost(KimidoriRoutePrefixes.Game + "/communicationlog.php")]
    [Produces("application/protobuf")]
    public IActionResult CommunicationLog([FromBody] CommunicationLogRequest request)
    {
        Logger.LogInformation("Kimidori CommunicationLog request: {@Request}", request);
        return Ok(new CommunicationLogResponse
        {
            Result = 1
        });
    }
}
