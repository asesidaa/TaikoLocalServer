namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Controllers;

[ApiController]
public class GetFolderController : BaseProtocolController<GetFolderController>
{
    [HttpPost("/v12r00_cn/chassis/getfolder.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetFolder([FromBody] GetfolderRequest request)
    {
        Logger.LogInformation("GetFolderCN00 request : {Request}", request.Stringify());
        var commonResponse = await Mediator.Send(new GetFolderQuery(GameEra.Nijiiro, request.FolderIds), HttpContext.RequestAborted);
        var response = FolderDataMappers.MapToCN00(commonResponse);
        return Ok(response);
    }
}
