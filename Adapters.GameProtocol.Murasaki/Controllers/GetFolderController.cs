namespace TaikoLocalServer.Adapters.GameProtocol.Murasaki.Controllers;

[ApiController]
public sealed class GetFolderController : BaseProtocolController<GetFolderController>
{
    [HttpPost(MurasakiRoutePrefixes.Game + "/getfolder.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetFolder([FromBody] GetfolderRequest request)
    {
        Logger.LogInformation("Murasaki GetFolder request: {@Request}", request);
        var common = await Mediator.Send(
            new GetFolderQuery(GameEra.Murasaki, [request.FolderId]),
            HttpContext.RequestAborted);
        return Ok(FolderDataMappers.MapSingle(common, request.FolderId));
    }
}
