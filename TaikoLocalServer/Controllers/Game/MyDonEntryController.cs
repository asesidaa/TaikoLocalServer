using TaikoLocalServer.Handlers;
using TaikoLocalServer.Mappers;

namespace TaikoLocalServer.Controllers.Game;

[ApiController]
public class MyDonEntryController : BaseController<MyDonEntryController>
{
    [HttpPost("/v12r08_ww/chassis/mydonentry_3nrd7kwk.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetMyDonEntry([FromBody] MydonEntryRequest request)
    {
        Logger.LogInformation("MyDonEntry request : {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new AddMyDonEntryCommand(request.AccessCode, request.MydonName, request.MydonNameLanguage));
        var response = MyDonEntryMappers.MapTo3906(commonResponse);
        return Ok(response);
    }
    
    [HttpPost("/v12r00_cn/chassis/mydonentry.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetMyDonEntry3209([FromBody] Models.v3209.MydonEntryRequest request)
    {
        Logger.LogInformation("MyDonEntry request : {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new AddMyDonEntryCommand(request.WechatQrStr, request.MydonName, request.MydonNameLanguage));
        var response = MyDonEntryMappers.MapTo3209(commonResponse);
        return Ok(response);
    }
}