using GameDatabase.Entities;

namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r03/chassis/getdanscore.php")]
[ApiController]
public class GetDanScoreController : BaseController<GetDanScoreController>
{
    private readonly IDanScoreDatumService danScoreDatumService;

    public GetDanScoreController(IDanScoreDatumService danScoreDatumService)
    {
        this.danScoreDatumService = danScoreDatumService;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetDanScore([FromBody] GetDanScoreRequest request)
    {
        Logger.LogInformation("GetDanScore request : {Request}", request.Stringify());

        var response = new GetDanScoreResponse
        {
            Result = 1
        };

        var danScoreData = await danScoreDatumService.GetDanScoreDatumByBaid(request.Baid);

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
                responseData.AryDanScoreDataStages.Add(ObjectMappers.DanStageDbToResponseMap.Apply(stageScoreDatum));

            response.AryDanScoreDatas.Add(responseData);
        }

        return Ok(response);
    }
}