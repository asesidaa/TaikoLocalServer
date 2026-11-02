namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Controllers;

[ApiController]
public class GetSongIntroductionController : BaseProtocolController<GetSongIntroductionController>
{
    private readonly IGameDataCatalog gameDataService;


    public GetSongIntroductionController(IGameDataCatalog gameDataService)
    {
        this.gameDataService = gameDataService;
    }

    [HttpPost("/v12r08_ww/chassis/getsongintroduction_66blw6is.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetSongIntroduction([FromBody] GetSongIntroductionRequest request)
    {
        Logger.LogInformation("GetSongIntroduction request : {@Request}", request);

        var commonResponse = await Mediator.Send(new GetSongIntroductionQuery(GameEra.Nijiiro, request.SetIds), HttpContext.RequestAborted);
        var response = SongIntroductionDataMappers.MapToWW08(commonResponse);

        return Ok(response);
    }
}
