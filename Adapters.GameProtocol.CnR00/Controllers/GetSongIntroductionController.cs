namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Controllers;

[ApiController]
public class GetSongIntroductionController : BaseProtocolController<GetSongIntroductionController>
{
    private readonly IGameDataCatalog gameDataService;


    public GetSongIntroductionController(IGameDataCatalog gameDataService)
    {
        this.gameDataService = gameDataService;
    }

    [HttpPost("/v12r00_cn/chassis/getsongintroduction.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetSongIntroductionCN00([FromBody] GetSongIntroductionRequest request)
    {
        Logger.LogInformation("GetSongIntroduction request : {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new GetSongIntroductionQuery(GameEra.Nijiiro, request.SetIds), HttpContext.RequestAborted);
        var response = SongIntroductionDataMappers.MapToCN00(commonResponse);

        return Ok(response);
    }
}
