namespace TaikoLocalServer.Adapters.GameProtocol.White.Controllers;

[ApiController]
[Route("/v07r00/chassis/recommend.php")]
public sealed class RecommendController : BaseProtocolController<RecommendController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult Recommend([FromBody] RecommendRequest request)
    {
        Logger.LogInformation(
            "White scaffold recommend.php request: ChassisId={ChassisId}, ShopId={ShopId}, GenderType={GenderType}, PlayerAge={PlayerAge}",
            request.ChassisId,
            request.ShopId,
            request.GenderType,
            request.PlayerAge);
        return Ok(new RecommendResponse { Result = 1 });
    }
}
