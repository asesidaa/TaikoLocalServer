using TaikoLocalServer.Mappers;

namespace TaikoLocalServer.Controllers.Game;

[ApiController]
public class GetSongIntroductionController : BaseController<GetSongIntroductionController>
{
    private readonly IGameDataService gameDataService;

    public GetSongIntroductionController(IGameDataService gameDataService)
    {
        this.gameDataService = gameDataService;
    }

    [HttpPost("/v12r08_ww/chassis/getsongintroduction_66blw6is.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetSongIntroduction([FromBody] GetSongIntroductionRequest request)
    {
        Logger.LogInformation("GetSongIntroduction request : {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new GetSongIntroductionQuery(request.SetIds), HttpContext.RequestAborted);
        var response = SongIntroductionDataMappers.MapToWW08(commonResponse);
        
        return Ok(response);
    }
    
    [HttpPost("/v12r00_cn/chassis/getsongintroduction.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetSongIntroductionCN00([FromBody] Models.CN00.GetSongIntroductionRequest request)
    {
        Logger.LogInformation("GetSongIntroduction request : {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new GetSongIntroductionQuery(request.SetIds), HttpContext.RequestAborted);
        var response = SongIntroductionDataMappers.MapToCN00(commonResponse);
        
        return Ok(response);
    }
}