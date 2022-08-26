namespace TaikoLocalServer.Controllers;

[Route("/v12r03/chassis/getsongintroduction.php")]
[ApiController]
public class GetSongIntroductionController : ControllerBase
{
    private readonly ILogger<GetSongIntroductionController> logger;
    public GetSongIntroductionController(ILogger<GetSongIntroductionController> logger) {
        this.logger = logger;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetSongIntroduction([FromBody] GetSongIntroductionRequest request)
    {
        logger.LogInformation("GetSongIntroduction request : {Request}", request.Stringify());

        var response = new GetSongIntroductionResponse
        {
            Result = 1
        };

        foreach (var setId in request.SetIds)
        {
            response.ArySongIntroductionDatas.Add(new GetSongIntroductionResponse.SongIntroductionData
            {
                MainSongNo = 2,
                SubSongNoes = new uint[] {177,193,3,4},
                SetId = setId,
                VerupNo = 1
            });
        }

        return Ok(response);
    }
}