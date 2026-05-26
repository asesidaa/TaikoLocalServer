namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/getitemshopinfo.php")]
public class GetItemShopInfoController : BaseProtocolController<GetItemShopInfoController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetItemShopInfo([FromBody] GetitemshopinfoRequest request)
    {
        Logger.LogInformation("Blue GetItemShopInfo request: {Request}", request.Stringify());
        return Ok(new GetitemshopinfoResponse { Result = 1 });
    }
}
