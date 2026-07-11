namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Controllers;

[ApiController]
public class GetTelopController : BaseProtocolController<GetTelopController>
{
    [HttpPost("/v12r00_cn/chassis/gettelop.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetTelopCN00([FromBody] GettelopRequest request)
    {
        Logger.LogInformation("GetTelop request : {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new GetTelopQuery(request.TelopId), HttpContext.RequestAborted);

        var response = new GettelopResponse
        {
            Result = commonResponse.Result,
            StartDatetime = commonResponse.StartDatetime,
            EndDatetime = commonResponse.EndDatetime,
            Telop = commonResponse.Telop,
            VerupNo = commonResponse.VerupNo
        };

        return Ok(response);
    }
}
