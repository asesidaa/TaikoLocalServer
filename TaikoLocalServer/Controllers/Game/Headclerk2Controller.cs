namespace TaikoLocalServer.Controllers.Game;

[ApiController]
[Route("/v12r00_cn/chassis/headclerk2.php")]
public class Headclerk2Controller : BaseController<Headclerk2Controller>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Headclerk2([FromBody] HeadClerk2Request request)
    {
        Logger.LogInformation("Headclerk2 request: {Request}", request.Stringify());

        /*var chassisId = request.ChassisId;
        var shopId = request.ShopId;
        foreach (var playInfo in request.AryPlayInfoes)
        {
            var baid = playInfo.Baid;
            var playedAt = playInfo.PlayedAt;
            var isRight = playInfo.IsRight;
            var type = playInfo.Type;
            var amount = playInfo.Amount;
            Logger.LogInformation("CSV WRITE: \n" +
                                  "ChassisId:{ChassisId},\n" +
                                  "ShopId:{ShopId},\n" +
                                  "Baid:{Baid},\n" +
                                  "PlayedAt{PlayedAt},\n" +
                                  "IsRight:{IsRight},\n" +
                                  "Type:{Type},\n" +
                                  "Amount{Amount}", chassisId, shopId, baid, playedAt, isRight, type, amount);
        }*/
		
        var response = new HeadClerk2Response
        {
            Result = 1
        };

        return Ok(response);
    }
}