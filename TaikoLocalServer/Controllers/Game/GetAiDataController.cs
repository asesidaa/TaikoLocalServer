namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r03/chassis/getaidata.php")]
[ApiController]
public class GetAiDataController : BaseController<GetAiDataController>
{
    private readonly IAiDatumService aiDatumService;

    public GetAiDataController(IAiDatumService aiDatumService)
    {
        this.aiDatumService = aiDatumService;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetAiData([FromBody] GetAiDataRequest request)
    {
        Logger.LogInformation("GetAiData request : {Request}", request.Stringify());

        var aiScoreData = await aiDatumService.GetAllAiScoreById(request.Baid);
        var totalWin = aiScoreData.Count(datum => datum.IsWin);
        var response = new GetAiDataResponse
        {
            Result  = 1,
            TotalWinnings = (uint)totalWin,
            InputMedian = "1000",
            InputVariance = "2000"
        };

        return Ok(response);
    }
}