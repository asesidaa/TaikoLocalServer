using Microsoft.EntityFrameworkCore;
using Swan.Mapping;

namespace TaikoLocalServer.Controllers;

[Route("/v12r03/chassis/getdanscore.php")]
[ApiController]
public class GetDanScoreController : ControllerBase
{
    private readonly ILogger<GetDanScoreController> logger;
    
    private readonly TaikoDbContext context;

    public GetDanScoreController(ILogger<GetDanScoreController> logger, TaikoDbContext context)
    {
        this.logger = logger;
        this.context = context;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetDanScore([FromBody] GetDanScoreRequest request)
    {
        logger.LogInformation("GetDanScore request : {Request}", request.Stringify());

        var response = new GetDanScoreResponse
        {
            Result = 1
        };

        var danScoreData = context.DanScoreData
            .Where(datum => datum.Baid == request.Baid)
            .Include(datum => datum.DanStageScoreData)
            .ToList();

        foreach (var danId in request.DanIds)
        {
            var datum = danScoreData.FirstOrDefault(scoreDatum => scoreDatum.DanId == danId, new DanScoreDatum());

            var responseData = new GetDanScoreResponse.DanScoreData
            {
                DanId = danId,
                ArrivalSongCnt = datum.ArrivalSongCount,
                ComboCntTotal = datum.ComboCountTotal,
                SoulGaugeTotal = datum.SoulGaugeTotal
            };
            foreach (var stageScoreDatum in datum.DanStageScoreData)
            {
                responseData.AryDanScoreDataStages.Add(ObjectMappers.DanStageDataMap.Apply(stageScoreDatum));
            }
            
            response.AryDanScoreDatas.Add(responseData);
        }

        return Ok(response);
    }
}