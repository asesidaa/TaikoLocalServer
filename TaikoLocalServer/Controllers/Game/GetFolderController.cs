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
        var response = FolderDataMappers.MapTo3906(commonResponse);
        return Ok(response);
    }
    
    [HttpPost("/v12r00_cn/chassis/getfolder.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetFolder([FromBody] Models.v3209.GetfolderRequest request)
    {
        Logger.LogInformation("GetFolder3209 request : {Request}", request.Stringify());
        var commonResponse = await Mediator.Send(new GetFolderQuery(request.FolderIds));
        var response = FolderDataMappers.MapTo3209(commonResponse);
        return Ok(response);
    }
}