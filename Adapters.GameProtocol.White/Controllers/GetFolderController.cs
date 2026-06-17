namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
[Route("/v07r00/chassis/getfolder.php")]
public sealed class GetFolderController : BaseProtocolController<GetFolderController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetFolder([FromBody] GetfolderRequest request)
    {
        Logger.LogInformation(
            "White scaffold getfolder.php request: ChassisId={ChassisId}, ShopId={ShopId}, HddVer={HddVer}, FolderCount={FolderCount}",
            request.ChassisId,
            request.ShopId,
            request.HddVer,
            request.FolderIds?.Length ?? 0);
        return Ok(new GetfolderResponse { Result = 1 });
    }
}
