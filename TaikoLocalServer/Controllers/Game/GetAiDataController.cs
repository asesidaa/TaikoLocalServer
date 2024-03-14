using TaikoLocalServer.Handlers;
using Throw;

namespace TaikoLocalServer.Controllers.Game;

[ApiController]
public class GetAiDataController : BaseController<GetAiDataController>
{
    private readonly IUserDatumService userDatumService;

    public GetAiDataController(IUserDatumService userDatumService)
    {
        this.userDatumService = userDatumService;
    }

    [HttpPost("/v12r08_ww/chassis/getaidata_6x30b9nr.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetAiData([FromBody] GetAiDataRequest request)
    {
        Logger.LogInformation("GetAiData request : {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new GetAiDataQuery(request.Baid));
        var response = Mappers.AiDataResponseMapper.MapTo3906(commonResponse);
        response.Result = 1;
        return Ok(response);
    }
    
    [HttpPost("/v12r00_cn/chassis/getaidata.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetAiData3209([FromBody] Models.v3209.GetAiDataRequest request)
    {
        Logger.LogInformation("GetAiData request : {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new GetAiDataQuery((uint)request.Baid));
        var response = Mappers.AiDataResponseMapper.MapTo3209(commonResponse);
        response.Result = 1;
        return Ok(response);
    }
}