namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/getitemshopinfo.php")]
public class GetItemShopInfoController : BaseProtocolController<GetItemShopInfoController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetItemShopInfo([FromBody] GetitemshopinfoRequest request)
    {
        Logger.LogInformation("Blue GetItemShopInfo request: {Request}", request.Stringify());
        var common = await Mediator.Send(ItemShopMappers.Map(request), HttpContext.RequestAborted);
        return Ok(ItemShopMappers.Map(common));
    }
}
