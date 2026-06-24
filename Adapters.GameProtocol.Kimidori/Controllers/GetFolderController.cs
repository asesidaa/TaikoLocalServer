namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Controllers;

[ApiController]
public sealed class GetFolderController : BaseProtocolController<GetFolderController>
{
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
