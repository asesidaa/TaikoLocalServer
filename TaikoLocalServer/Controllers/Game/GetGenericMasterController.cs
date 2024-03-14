namespace TaikoLocalServer.Controllers.Game;

[ApiController]
public class GetGenericMasterController : BaseController<GetGenericMasterController>
{
    [HttpPost("/v12r08_ww/chassis/getgenericmaster_ts8om3qd.php")]
    [Produces("application/protobuf")]
    public IActionResult GetGenericMaster([FromBody] GetGenericMasterRequest request)
    {
        Logger.LogInformation("GetGenericMasterRequest: {Request}", request.Stringify());

        var response = new GetGenericMasterResponse
        {
            Result = 1,
            VerupNo = 2,
            EnableIdBit = FlagCalculator.GetBitArrayTrue(5000)
        };

        return Ok(response);
    }
    
    [HttpPost("/v12r00_cn/chassis/getgenericmaster.php")]
    [Produces("application/protobuf")]
    public IActionResult GetGenericMaster([FromBody] Models.v3209.GetGenericMasterRequest request)
    {
        Logger.LogInformation("GetGenericMaster3209Request: {Request}", request.Stringify());

        var response = new Models.v3209.GetGenericMasterResponse
        {
            Result = 1,
            VerupNo = 2,
            EnableIdBit = FlagCalculator.GetBitArrayTrue(5000)
        };

        return Ok(response);
    }

}