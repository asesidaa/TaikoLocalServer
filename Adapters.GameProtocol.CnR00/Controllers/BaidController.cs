namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Controllers;

[ApiController]
public class BaidController : BaseProtocolController<BaidController>
{
    [HttpPost("/v12r00_cn/chassis/baidcheck.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetBaidCN00([FromBody] BAIDRequest request)
    {
        Logger.LogInformation("Baid request: {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new BaidQuery(request.WechatQrStr), HttpContext.RequestAborted);
        BAIDResponse response;
        if (commonResponse.IsNewUser)
        {
            Logger.LogInformation("New user with access code {AccessCode}", request.WechatQrStr);

            response = new BAIDResponse
            {
                Result = 1,
                PlayerType = 1,
                Baid = commonResponse.Baid,
            };

            return Ok(response);
        }

        response = BaidResponseMapper.MapCN00WithPostProcess(commonResponse);
        response.PlayerType = 0;

        return Ok(response);
    }
}
