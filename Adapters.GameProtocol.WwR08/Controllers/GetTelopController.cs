namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Controllers;

[ApiController]
public class GetTelopController : BaseProtocolController<GetTelopController>
{
    [HttpPost("/v12r08_ww/chassis/gettelop_o0cb2z3e.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetTelop([FromBody] GettelopRequest request)
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
