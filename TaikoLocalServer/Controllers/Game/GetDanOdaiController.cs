﻿namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r00_cn/chassis/getdanodai.php")]
[ApiController]
public class GetDanOdaiController : BaseController<GetDanOdaiController>
{
    private readonly IGameDataService gameDataService;

    public GetDanOdaiController(IGameDataService gameDataService)
    {
        this.gameDataService = gameDataService;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetDanOdai([FromBody] GetDanOdaiRequest request)
    {
        Logger.LogInformation("GetDanOdai request : {Request}", request.Stringify());
        
        var response = new GetDanOdaiResponse
        {
            Result = 1
        };

        if (request.Type == 1)
        {
            foreach (var danId in request.DanIds)
            {
                gameDataService.GetDanDataDictionary().TryGetValue(danId, out var odaiData);
                if (odaiData is null)
                {
                    Logger.LogWarning("Requested dan id {Id} does not exist!", danId);
                    continue;
                }

                response.AryOdaiDatas.Add(odaiData);
            }
        }
        else if (request.Type == 2)
        {
            foreach (var danId in request.DanIds)
            {
                gameDataService.GetGaidenDataDictionary().TryGetValue(danId, out var odaiData);
                if (odaiData is null)
                {
                    Logger.LogWarning("Requested dan id {Id} does not exist!", danId);
                    continue;
                }

                response.AryOdaiDatas.Add(odaiData);
            }
        }
        
        return Ok(response);
    }
}