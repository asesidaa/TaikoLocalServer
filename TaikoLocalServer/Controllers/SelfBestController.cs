using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using TaikoLocalServer.Models;
using TaikoLocalServer.Utils;

namespace TaikoLocalServer.Controllers;

[Route("/v12r03/chassis/selfbest.php")]
[ApiController]
public class SelfBestController : ControllerBase
{
    private readonly ILogger<SelfBestController> logger;

    public SelfBestController(ILogger<SelfBestController> logger)
    {
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
            if (!manager.MusicAttributes.ContainsKey(songNo))
            {
                logger.LogWarning("Music no {No} is missing!", songNo);
                continue;
            }

            var selfBestData = new SelfBestResponse.SelfBestData
            {
                SongNo = songNo,
                SelfBestScore = 0,
                SelfBestScoreRate = 0,
                UraBestScore = 0,
                UraBestScoreRate = 0
            };
            response.ArySelfbestScores.Add(selfBestData);
        }
        response.ArySelfbestScores.Sort((data, otherData) => data.SongNo.CompareTo(otherData.SongNo));
        
        return Ok(response);
    }
}