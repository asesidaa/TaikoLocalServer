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

        var odaiDataList = await Mediator.Send(new GetDanOdaiQuery(request.DanIds, request.Type), HttpContext.RequestAborted);
        response.AryOdaiDatas.AddRange(odaiDataList.Select(DanDataMappers.ToWW08OdaiData));

        return Ok(response);
    }
    
    [HttpPost("/v12r00_cn/chassis/getdanodai.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetDanOdaiCN00([FromBody] Models.CN00.GetDanOdaiRequest request)
    {
        Logger.LogInformation("GetDanOdai request : {Request}", request.Stringify());

        var response = new Models.CN00.GetDanOdaiResponse
        {
            Result = 1
        };

        var odaiDataList = await Mediator.Send(new GetDanOdaiQuery(request.DanIds, request.Type), HttpContext.RequestAborted);
        response.AryOdaiDatas.AddRange(odaiDataList.Select(DanDataMappers.ToCN00OdaiData));

        return Ok(response);
    }
}