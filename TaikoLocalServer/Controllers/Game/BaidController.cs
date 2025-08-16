namespace TaikoLocalServer.Controllers.Game;

[ApiController]
public class BaidController : BaseController<BaidController>
{
    [HttpPost("/v12r08_ww/chassis/baidcheck_dcfxit1u.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetBaid([FromBody] BAIDRequest request)
    {
        Logger.LogInformation("Baid request: {Request}", request.Stringify());
        var commonResponse = await Mediator.Send(new BaidQuery(request.AccessCode));
        BAIDResponse response;
        if (commonResponse.IsNewUser)
        {
            Logger.LogInformation("New user with access code {AccessCode}", request.AccessCode);

            response = new BAIDResponse
            {
                Result = 1,
                PlayerType = 1,
                Baid = commonResponse.Baid,
            };

            return Ok(response);
        }

        response = Mappers.BaidResponseMapper.MapWW08WithPostProcess(commonResponse);
        response.PlayerType = 0;
        
        return Ok(response);
    }
    
    [HttpPost("/v12r00_cn/chassis/baidcheck.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetBaidCN00([FromBody] Models.CN00.BAIDRequest request)
    {
        Logger.LogInformation("Baid request: {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new BaidQuery(request.WechatQrStr));
        Models.CN00.BAIDResponse response;
        if (commonResponse.IsNewUser)
        {
            Logger.LogInformation("New user with access code {AccessCode}", request.WechatQrStr);

            response = new Models.CN00.BAIDResponse
            {
                Result = 1,
                PlayerType = 1,
                Baid = commonResponse.Baid,
            };

            return Ok(response);
        }

        response = Mappers.BaidResponseMapper.MapCN00WithPostProcess(commonResponse);
        response.PlayerType = 0;
        
        return Ok(response);
    }
}