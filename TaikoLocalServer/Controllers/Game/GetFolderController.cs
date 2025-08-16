using TaikoLocalServer.Mappers;

namespace TaikoLocalServer.Controllers.Game;

[ApiController]
public class GetFolderController : BaseController<GetFolderController>
{
    [HttpPost("/v12r08_ww/chassis/getfolder_rffj346i.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetFolder([FromBody] GetfolderRequest request)
    {
        Logger.LogInformation("GetFolder request : {Request}", request.Stringify());
        var commonResponse = await Mediator.Send(new GetFolderQuery(request.FolderIds));
        var response = FolderDataMappers.MapToWW08(commonResponse);
        return Ok(response);
    }
    
    [HttpPost("/v12r00_cn/chassis/getfolder.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetFolder([FromBody] Models.CN00.GetfolderRequest request)
    {
        Logger.LogInformation("GetFolderCN00 request : {Request}", request.Stringify());
        var commonResponse = await Mediator.Send(new GetFolderQuery(request.FolderIds));
        var response = FolderDataMappers.MapToCN00(commonResponse);
        return Ok(response);
    }
}