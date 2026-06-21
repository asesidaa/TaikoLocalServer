namespace TaikoLocalServer.Adapters.GameProtocol.Murasaki.Controllers;

[ApiController]
public sealed class TelopCheckController : BaseProtocolController<TelopCheckController>
{
    [HttpPost(MurasakiRoutePrefixes.Game + "/telopcheck.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> TelopCheck([FromBody] TelopcheckRequest request)
    {
        Logger.LogInformation("Murasaki TelopCheck request: {@Request}", request);
        var common = await Mediator.Send(new GetInitialDataQuery(GameEra.Murasaki), HttpContext.RequestAborted);
        return Ok(new TelopcheckResponse
        {
            Result = common.Result,
            TelopIds = common.AryTelopDatas.Select(row => row.InfoId).ToArray()
        });
    }
}
