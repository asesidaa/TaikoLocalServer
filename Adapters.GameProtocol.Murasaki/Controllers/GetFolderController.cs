using LegacyWire = TaikoLocalServer.Adapters.GameProtocol.Murasaki.LegacyWire;

namespace TaikoLocalServer.Adapters.GameProtocol.Murasaki.Controllers;

[ApiController]
public sealed class GetFolderController : BaseProtocolController<GetFolderController>
{
    [HttpPost(MurasakiRoutePrefixes.Final + "/getfolder.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetFolder([FromBody] GetfolderRequest request)
    {
        Logger.LogInformation("Murasaki GetFolder request: {@Request}", request);
        var common = await Mediator.Send(
            new GetFolderQuery(GameEra.Murasaki, request.FolderIds ?? []),
            HttpContext.RequestAborted);
        return Ok(FolderDataMappers.Map(common));
    }

    [HttpPost(MurasakiRoutePrefixes.Compatibility + "/getfolder.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> LegacyGetFolder([FromBody] LegacyWire.GetfolderRequest request)
    {
        Logger.LogInformation("Murasaki compatibility GetFolder request: {@Request}", request);
        var common = await Mediator.Send(
            new GetFolderQuery(GameEra.Murasaki, [request.FolderId]),
            HttpContext.RequestAborted);
        return Ok(LegacyWire.LegacyFolderDataMappers.MapSingle(common, request.FolderId));
    }
}
