namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Controllers;

[ApiController]
public class MyDonEntryController : BaseProtocolController<MyDonEntryController>
{
    [HttpPost("/v12r00_cn/chassis/mydonentry.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetMyDonEntryCN00([FromBody] MydonEntryRequest request)
    {
        Logger.LogInformation("MyDonEntry request : {@Request}", request);

        var commonResponse = await Mediator.Send(new AddMyDonEntryCommand(GameEra.Nijiiro, request.WechatQrStr, request.MydonName, request.MydonNameLanguage), HttpContext.RequestAborted);
        var response = MyDonEntryMappers.MapToCN00(commonResponse);
        return Ok(response);
    }
}
