namespace TaikoLocalServer.Controllers;

[Route("/v12r03/chassis/selfbest.php")]
[ApiController]
public class SelfBestController : ControllerBase
{
    private readonly ILogger<SelfBestController> logger;

    private readonly TaikoDbContext context;
    
    public SelfBestController(ILogger<SelfBestController> logger, TaikoDbContext context)
    {
        this.logger = logger;
        this.context = context;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult SelfBest([FromBody] SelfBestRequest request)
    {
        logger.LogInformation("SelfBest request : {Request}", request.Stringify());

        var response = new SelfBestResponse
        {
            Result = 1,
            Level = request.Level
        };

        var manager = MusicAttributeManager.Instance;
        var difficulty = (Difficulty)request.Level;
        var playerBestData = context.SongBestData
            .Where(datum => datum.Baid == request.Baid &&
                            datum.Difficulty == difficulty)
            .ToList();
        foreach (var songNo in request.ArySongNoes)
        {
            if (!manager.MusicAttributes.ContainsKey(songNo))
            {
                logger.LogWarning("Music no {No} is missing!", songNo);
                continue;
            }

            var songBestDatum = playerBestData.FirstOrDefault(datum => datum.SongId == songNo, new SongBestDatum());

            var selfBestData = new SelfBestResponse.SelfBestData
            {
                SongNo = songNo,
                SelfBestScore = difficulty != Difficulty.UraOni ? songBestDatum.BestScore : 0,
                SelfBestScoreRate = difficulty != Difficulty.UraOni ? songBestDatum.BestRate : 0,
                UraBestScore = difficulty == Difficulty.UraOni ? songBestDatum.BestScore : 0,
                UraBestScoreRate = difficulty == Difficulty.UraOni ? songBestDatum.BestRate : 0
            };
            response.ArySelfbestScores.Add(selfBestData);
        }
        response.ArySelfbestScores.Sort((data, otherData) => data.SongNo.CompareTo(otherData.SongNo));
        
        return Ok(response);
    }
}