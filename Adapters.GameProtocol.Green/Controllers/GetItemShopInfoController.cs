namespace TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;

[ApiController]
[Route("/v11r01/chassis/getitemshopinfo.php")]
public class GetItemShopInfoController : BaseProtocolController<GetItemShopInfoController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetItemShopInfo([FromBody] GetitemshopinfoRequest request)
    {
        Logger.LogInformation("Green GetItemShopInfo request: {@Request}", request);
        var common = await Mediator.Send(ItemShopMappers.Map(request), HttpContext.RequestAborted);
        return Ok(ItemShopMappers.Map(common));
    }
}
