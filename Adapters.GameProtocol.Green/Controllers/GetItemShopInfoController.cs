namespace TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;

[ApiController]
[Route("/v11r01/chassis/getitemshopinfo.php")]
public class GetItemShopInfoController : BaseProtocolController<GetItemShopInfoController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetItemShopInfo([FromBody] GetitemshopinfoRequest request)
    {
        Logger.LogInformation("Green GetItemShopInfo request: {Request}", request.Stringify());
        return Ok(new GetitemshopinfoResponse
        {
            Result = 1
        });
    }
}
