namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Controllers;

[ApiController]
public class GetGenericMasterController : BaseProtocolController<GetGenericMasterController>
{
    [HttpPost("/v12r00_cn/chassis/getgenericmaster.php")]
    [Produces("application/protobuf")]
    public IActionResult GetGenericMaster([FromBody] GetGenericMasterRequest request)
    {
        Logger.LogInformation("GetGenericMasterCN00Request: {@Request}", request);

        var response = new GetGenericMasterResponse
        {
            Result = 1,
            VerupNo = 2,
            EnableIdBit = FlagCalculator.GetBitArrayTrue(5000)
        };

        return Ok(response);
    }
}
