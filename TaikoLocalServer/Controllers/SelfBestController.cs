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
                            (datum.Difficulty == difficulty || (datum.Difficulty == Difficulty.UraOni && difficulty == Difficulty.Oni)))
            .ToList();
        foreach (var songNo in request.ArySongNoes)
        {
            if (!manager.MusicAttributes.ContainsKey(songNo))
            {
                logger.LogWarning("Music no {No} is missing!", songNo);
                continue;
            }

            var songBestDatum = playerBestData.Where(datum => datum.SongId == songNo);

            var selfBestData = new SelfBestResponse.SelfBestData
            {
                SongNo = songNo,
            };

            foreach (var datum in songBestDatum)
            {
                if (datum.Difficulty == difficulty)
                {
                    selfBestData.SelfBestScore = datum.BestScore;
                    selfBestData.SelfBestScoreRate = datum.BestRate;
                }
                else if (datum.Difficulty == Difficulty.UraOni)
                {
                    selfBestData.UraBestScore = datum.BestScore;
                    selfBestData.UraBestScoreRate = datum.BestRate;
                }
            }

            response.ArySelfbestScores.Add(selfBestData);
        }
        response.ArySelfbestScores.Sort((data, otherData) => data.SongNo.CompareTo(otherData.SongNo));
        
        return Ok(response);
    }
}