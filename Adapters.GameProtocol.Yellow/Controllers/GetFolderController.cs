namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Controllers;

[ApiController]
[Route("/v09r02/chassis/getfolder.php")]
public class GetFolderController : BaseProtocolController<GetFolderController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetFolder([FromBody] GetfolderRequest request)
    {
        Logger.LogInformation("Yellow GetFolder request: {@Request}", request);
        var common = await Mediator.Send(
            new GetFolderQuery(GameEra.Yellow, request.FolderIds ?? []),
            HttpContext.RequestAborted);
        return Ok(FolderDataMappers.Map(common));
    }
}
