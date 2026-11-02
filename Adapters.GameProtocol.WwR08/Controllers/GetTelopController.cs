namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Controllers;

[ApiController]
public class GetTelopController : BaseProtocolController<GetTelopController>
{
    [HttpPost("/v12r08_ww/chassis/gettelop_o0cb2z3e.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetTelop([FromBody] GettelopRequest request)
    {
        Logger.LogInformation("GetTelop request : {@Request}", request);

        var commonResponse = await Mediator.Send(new GetTelopQuery(GameEra.Nijiiro, request.TelopId), HttpContext.RequestAborted);

        var response = new GettelopResponse
        {
            Result = commonResponse.Result,
            StartDatetime = commonResponse.StartDatetime,
            EndDatetime = commonResponse.EndDatetime,
            Telop = commonResponse.Telop,
            VerupNo = commonResponse.VerupNo ?? 0
        };

        return Ok(response);
    }
}
