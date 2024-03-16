using TaikoLocalServer.Handlers;
using TaikoLocalServer.Mappers;

namespace TaikoLocalServer.Controllers.Game;

[ApiController]
public class GetDanOdaiController : BaseController<GetDanOdaiController>
{
    [HttpPost("/v12r08_ww/chassis/getdanodai_ela9zu1a.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetDanOdai([FromBody] GetDanOdaiRequest request)
    {
        Logger.LogInformation("GetDanOdai request : {Request}", request.Stringify());

        var response = new GetDanOdaiResponse
        {
            Result = 1
        };

        var odaiDataList = await Mediator.Send(new GetDanOdaiQuery(request.DanIds, request.Type));
        response.AryOdaiDatas.AddRange(odaiDataList.Select(DanDataMappers.To3906OdaiData));

        return Ok(response);
    }
    
    [HttpPost("/v12r00_cn/chassis/getdanodai.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetDanOdai3209([FromBody] Models.v3209.GetDanOdaiRequest request)
    {
        Logger.LogInformation("GetDanOdai request : {Request}", request.Stringify());

        var response = new Models.v3209.GetDanOdaiResponse
        {
            Result = 1
        };

        var odaiDataList = await Mediator.Send(new GetDanOdaiQuery(request.DanIds, request.Type));
        response.AryOdaiDatas.AddRange(odaiDataList.Select(DanDataMappers.To3209OdaiData));

        return Ok(response);
    }
}