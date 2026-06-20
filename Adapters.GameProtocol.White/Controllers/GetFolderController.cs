using LegacyWire = TaikoLocalServer.Adapters.GameProtocol.White.LegacyWire;

namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
public sealed class GetFolderController : BaseProtocolController<GetFolderController>
{
    [HttpPost(WhiteRoutePrefixes.Final + "/getfolder.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetFolder([FromBody] GetfolderRequest request)
    {
        Logger.LogInformation("White GetFolder request: {@Request}", request);
        var common = await Mediator.Send(
            new GetFolderQuery(GameEra.White, request.FolderIds ?? []),
            HttpContext.RequestAborted);
        return Ok(FolderDataMappers.Map(common));
    }

    [HttpPost(WhiteRoutePrefixes.Compatibility + "/getfolder.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> LegacyGetFolder([FromBody] LegacyWire.GetfolderRequest request)
    {
        Logger.LogInformation("White legacy GetFolder request: {@Request}", request);
        var common = await Mediator.Send(
            new GetFolderQuery(GameEra.White, request.FolderIds ?? []),
            HttpContext.RequestAborted);
        return Ok(LegacyWire.LegacyFolderDataMappers.Map(common));
    }
}
