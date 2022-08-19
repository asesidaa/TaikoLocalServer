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

        if (request.ArySongNoes.Length == 1)
        {
            response.ArySelfbestScores.Add(new SelfBestResponse.SelfBestData
            {
                SongNo = request.ArySongNoes[0],
                SelfBestScore = 0,
                SelfBestScoreRate = 0,
                UraBestScore = 0,
                UraBestScoreRate = 0
            });
            return Ok(response);
        }
        var crashing = new HashSet<uint> { 2, 4, 47, 228, 232, 245, 255, 290, 385, 453, 464, 732, 772, 789, 790, 811, 824, 825, 837, 838, 840, 854, 861, 862, 877, 883, 885, 893, 894, 901 };
        var manager = MusicAttributeManager.Instance;
        /*foreach (var songNo in request.ArySongNoes)
        {
            if (!manager.MusicAttributes.ContainsKey(songNo))
            {
                logger.LogWarning("Music no {No} is missing!", songNo);
                continue;
            }

            if (crashing.Contains(songNo))
            {
                logger.LogWarning("Possible crashing song no!");
                continue;
            }
            var selfBestData = new SelfBestResponse.SelfBestData
            {
                SelfBestScore = 0,
                SelfBestScoreRate = 0,
                SongNo = songNo,
                UraBestScore = 0,
                UraBestScoreRate = 0
            };
            response.ArySelfbestScores.Add(selfBestData);
        }*/
        response.ArySelfbestScores.Sort((data, otherData) => data.SongNo.CompareTo(otherData.SongNo));

        return Ok(response);
    }
}