namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Controllers;

using FinalWire = TaikoLocalServer.Adapters.GameProtocol.Kimidori.FinalWire;

[ApiController]
public sealed class GetFolderController : BaseProtocolController<GetFolderController>
{
    [HttpPost(KimidoriRoutePrefixes.Final + "/getfolder.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> FinalGetFolder([FromBody] FinalWire.GetfolderRequest request)
    {
        Logger.LogInformation("Kimidori final GetFolder request: {@Request}", request);
        var common = await Mediator.Send(
            new GetFolderQuery(GameEra.Kimidori, [request.FolderId]),
            HttpContext.RequestAborted);
        return Ok(FinalFolderDataMappers.MapSingle(common, request.FolderId));
    }

    [HttpPost(KimidoriRoutePrefixes.Game + "/getfolder.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetFolder([FromBody] GetfolderRequest request)
    {
        Logger.LogInformation("Kimidori GetFolder request: {@Request}", request);
        var common = await Mediator.Send(
            new GetFolderQuery(GameEra.Kimidori, [request.FolderId]),
            HttpContext.RequestAborted);
        return Ok(FolderDataMappers.MapSingle(common, request.FolderId));
    }
}
