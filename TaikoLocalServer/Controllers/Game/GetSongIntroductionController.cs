namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r03/chassis/getsongintroduction.php")]
[ApiController]
public class GetSongIntroductionController : BaseController<GetSongIntroductionController>
{
    private readonly IGameDataService gameDataService;

    public GetSongIntroductionController(IGameDataService gameDataService)
    {
        this.gameDataService = gameDataService;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetSongIntroduction([FromBody] GetSongIntroductionRequest request)
    {
        Logger.LogInformation("GetSongIntroduction request : {Request}", request.Stringify());

        var response = new GetSongIntroductionResponse
        {
            Result = 1
        };

        foreach (var setId in request.SetIds)
        {
            gameDataService.GetSongIntroDictionary().TryGetValue(setId, out var introData);
            if (introData is null)
            {
                Logger.LogWarning("Requested set id {Id} does not exist!", setId);
                continue;
            }

            response.ArySongIntroductionDatas.Add(introData);
        }

        return Ok(response);
    }
}