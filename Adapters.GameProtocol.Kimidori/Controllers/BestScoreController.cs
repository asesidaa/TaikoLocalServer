namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Controllers;

[ApiController]
public sealed class BestScoreController : BaseProtocolController<BestScoreController>
{
    [HttpPost(KimidoriRoutePrefixes.Game + "/bestscore.php")]
    [Produces("application/protobuf")]
    public IActionResult BestScore([FromBody] BestScoreRequest request)
    {
        Logger.LogInformation("Kimidori BestScore request: {@Request}", request);
        return Ok(new BestScoreResponse
        {
            Result = 1,
            SeqId = request.SeqId,
            LastSeqId = request.SeqId
        });
    }
}
