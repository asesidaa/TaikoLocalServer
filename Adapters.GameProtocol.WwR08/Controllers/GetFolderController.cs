namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Controllers;

[ApiController]
public class GetFolderController : BaseProtocolController<GetFolderController>
{
    [HttpPost("/v12r08_ww/chassis/getfolder_rffj346i.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetFolder([FromBody] GetfolderRequest request)
    {
        Logger.LogInformation("GetFolder request : {@Request}", request);
        var commonResponse = await Mediator.Send(new GetFolderQuery(GameEra.Nijiiro, request.FolderIds), HttpContext.RequestAborted);
        var response = FolderDataMappers.MapToWW08(commonResponse);
        return Ok(response);
    }
}
