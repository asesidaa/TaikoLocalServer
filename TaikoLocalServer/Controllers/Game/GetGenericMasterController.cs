namespace TaikoLocalServer.Controllers.Game;

[ApiController]
[Route("/v12r08_ww/chassis/getgenericmaster_ts8om3qd.php")]
public class GetGenericMasterController : BaseController<GetGenericMasterController>
{
    [HttpPost]
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

}