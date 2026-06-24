namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Controllers;

[ApiController]
public sealed class TelopCheckController : BaseProtocolController<TelopCheckController>
{
    [HttpPost(KimidoriRoutePrefixes.Game + "/telopcheck.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> TelopCheck([FromBody] TelopcheckRequest request)
    {
        Logger.LogInformation("Kimidori TelopCheck request: {@Request}", request);
        var common = await Mediator.Send(new GetInitialDataQuery(GameEra.Kimidori), HttpContext.RequestAborted);
        return Ok(new TelopcheckResponse
        {
            Result = common.Result,
            TelopIds = common.AryTelopDatas.Select(row => row.InfoId).ToArray()
        });
    }
}
