namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Controllers;

[ApiController]
public class GetGenericMasterController : BaseProtocolController<GetGenericMasterController>
{
    [HttpPost("/v12r08_ww/chassis/getgenericmaster_ts8om3qd.php")]
    [Produces("application/protobuf")]
    public IActionResult GetGenericMaster([FromBody] GetGenericMasterRequest request)
    {
        Logger.LogInformation("GetGenericMasterRequest: {@Request}", request);

        var response = new GetGenericMasterResponse
        {
            Result = 1,
            VerupNo = 2,
            EnableIdBit = FlagCalculator.GetBitArrayTrue(5000)
        };

        return Ok(response);
    }
}
