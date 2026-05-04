namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Controllers;

[ApiController]
public class GetDanOdaiController : BaseProtocolController<GetDanOdaiController>
{
    [HttpPost("/v12r00_cn/chassis/getdanodai.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetDanOdaiCN00([FromBody] GetDanOdaiRequest request)
    {
        Logger.LogInformation("GetDanOdai request : {Request}", request.Stringify());

        var response = new GetDanOdaiResponse
        {
            Result = 1
        };

        var odaiDataList = await Mediator.Send(new GetDanOdaiQuery(request.DanIds, request.Type), HttpContext.RequestAborted);
        response.AryOdaiDatas.AddRange(odaiDataList.Select(DanDataMappers.ToCN00OdaiData));

        return Ok(response);
    }
}
