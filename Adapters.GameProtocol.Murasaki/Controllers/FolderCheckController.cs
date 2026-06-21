namespace TaikoLocalServer.Adapters.GameProtocol.Murasaki.Controllers;

[ApiController]
public sealed class FolderCheckController : BaseProtocolController<FolderCheckController>
{
    [HttpPost(MurasakiRoutePrefixes.Game + "/foldercheck.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> FolderCheck([FromBody] FoldercheckRequest request)
    {
        Logger.LogInformation("Murasaki FolderCheck request: {@Request}", request);
        var common = await Mediator.Send(new GetInitialDataQuery(GameEra.Murasaki), HttpContext.RequestAborted);
        return Ok(new FoldercheckResponse
        {
            Result = common.Result,
            FolderIds = common.AryEventFolderDatas.Select(row => row.InfoId).ToArray()
        });
    }
}
