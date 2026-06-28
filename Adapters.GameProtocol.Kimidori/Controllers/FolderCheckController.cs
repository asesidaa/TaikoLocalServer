namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Controllers;

using FinalWire = TaikoLocalServer.Adapters.GameProtocol.Kimidori.FinalWire;

[ApiController]
public sealed class FolderCheckController : BaseProtocolController<FolderCheckController>
{
    [HttpPost(KimidoriRoutePrefixes.Final + "/foldercheck.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> FinalFolderCheck([FromBody] FinalWire.FoldercheckRequest request)
    {
        Logger.LogInformation("Kimidori final FolderCheck request: {@Request}", request);
        var common = await Mediator.Send(new GetInitialDataQuery(GameEra.Kimidori), HttpContext.RequestAborted);
        return Ok(new FinalWire.FoldercheckResponse
        {
            Result = common.Result,
            FolderIds = common.AryEventFolderDatas.Select(row => row.InfoId).ToArray()
        });
    }

    [HttpPost(KimidoriRoutePrefixes.Game + "/foldercheck.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> FolderCheck([FromBody] FoldercheckRequest request)
    {
        Logger.LogInformation("Kimidori FolderCheck request: {@Request}", request);
        var common = await Mediator.Send(new GetInitialDataQuery(GameEra.Kimidori), HttpContext.RequestAborted);
        return Ok(new FoldercheckResponse
        {
            Result = common.Result,
            FolderIds = common.AryEventFolderDatas.Select(row => row.InfoId).ToArray()
        });
    }
}
