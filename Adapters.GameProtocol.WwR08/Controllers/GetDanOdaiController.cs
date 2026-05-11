namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Controllers;

[ApiController]
public class GetDanOdaiController : BaseProtocolController<GetDanOdaiController>
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

        var odaiDataList = await Mediator.Send(new GetDanOdaiQuery(GameEra.Nijiiro, request.DanIds, request.Type), HttpContext.RequestAborted);
        response.AryOdaiDatas.AddRange(odaiDataList.Select(DanDataMappers.ToWW08OdaiData));

        return Ok(response);
    }
}
