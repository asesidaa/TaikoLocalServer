namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Controllers;

[ApiController]
public class MyDonEntryController : BaseProtocolController<MyDonEntryController>
{
    [HttpPost("/v12r08_ww/chassis/mydonentry_3nrd7kwk.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetMyDonEntry([FromBody] MydonEntryRequest request)
    {
        Logger.LogInformation("MyDonEntry request : {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new AddMyDonEntryCommand(GameEra.Nijiiro, request.AccessCode, request.MydonName, request.MydonNameLanguage), HttpContext.RequestAborted);
        var response = MyDonEntryMappers.MapToWW08(commonResponse);
        return Ok(response);
    }
}
