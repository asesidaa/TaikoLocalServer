namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Controllers;

using FinalWire = TaikoLocalServer.Adapters.GameProtocol.Kimidori.FinalWire;

[ApiController]
public sealed class TelopCheckController : BaseProtocolController<TelopCheckController>
{
    [HttpPost(KimidoriRoutePrefixes.Final + "/telopcheck.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> FinalTelopCheck([FromBody] FinalWire.TelopcheckRequest request)
    {
        Logger.LogInformation("Kimidori final TelopCheck request: {@Request}", request);
        var common = await Mediator.Send(new GetInitialDataQuery(GameEra.Kimidori), HttpContext.RequestAborted);
        return Ok(new FinalWire.TelopcheckResponse
        {
            Result = common.Result,
            TelopIds = common.AryTelopDatas.Select(row => row.InfoId).ToArray()
        });
    }

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
