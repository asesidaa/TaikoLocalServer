namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r03/chassis/getdanscore.php")]
[ApiController]
public class GetDanScoreController : BaseController<GetDanScoreController>
{
    private readonly TaikoDbContext context;

    public GetDanScoreController(TaikoDbContext context)
    {
        this.context = context;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetDanScore([FromBody] GetDanScoreRequest request)
    {
        Logger.LogInformation("GetDanScore request : {Request}", request.Stringify());

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
                responseData.AryDanScoreDataStages.Add(ObjectMappers.DanStageDbToResponseMap.Apply(stageScoreDatum));
            }
            
            response.AryDanScoreDatas.Add(responseData);
        }

        return Ok(response);
    }
}