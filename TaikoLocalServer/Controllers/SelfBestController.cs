using Microsoft.AspNetCore.Http;
using TaikoLocalServer.Models;
using TaikoLocalServer.Utils;

namespace TaikoLocalServer.Controllers;

[Route("/v12r03/chassis/selfbest.php")]
[ApiController]
public class SelfBestController : ControllerBase
{
    private readonly ILogger<SelfBestController> logger;
    public SelfBestController(ILogger<SelfBestController> logger) {
        this.logger = logger;
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
        foreach (var songNo in request.ArySongNoes)
        {
            var selfBestData = new SelfBestResponse.SelfBestData
            {
                SelfBestScore = 0,
                SelfBestScoreRate = 0,
                SongNo = songNo,
            };
            response.ArySelfbestScores.Add(selfBestData);
            if (manager.MusicHasUra(songNo))
            {
                selfBestData.UraBestScore = 0;
                selfBestData.UraBestScoreRate = 0;
            }
        }

        return Ok(response);
    }
}