using Swan.Mapping;

namespace TaikoLocalServer.Controllers;

[Route("/v12r03/chassis/getdanodai.php")]
[ApiController]
public class GetDanOdaiController : ControllerBase
{
    private readonly ILogger<GetDanOdaiController> logger;
    public GetDanOdaiController(ILogger<GetDanOdaiController> logger) {
        this.logger = logger;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetDanOdai([FromBody] GetDanOdaiRequest request)
    {
        logger.LogInformation("GetDanOdai request : {Request}", request.Stringify());

        var response = new GetDanOdaiResponse
        {
            Result = 1
        };

        if (request.Type == 2)
        {
            return Ok(response);
        }

        var manager = DanOdaiDataManager.Instance;
        foreach (var danId in request.DanIds)
        {
            manager.OdaiDataList.TryGetValue(danId, out var odaiData);
            if (odaiData is null)
            {
                logger.LogWarning("Requested dan id {Id} does not exist!", danId);
                continue;
            }

            response.AryOdaiDatas.Add(odaiData);
        }

        return Ok(response);
    }
}