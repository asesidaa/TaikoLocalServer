using TaikoLocalServer.Handlers;

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

        response = Mappers.BaidResponseMapper.Map3906WithPostProcess(commonResponse);
        response.Result = 1;
        response.PlayerType = 0;
        response.IsDispAchievementTypeSet = true;
        response.IsDispSouuchiOn = true;
        
        return Ok(response);
    }
    
    [HttpPost("/v12r00_cn/chassis/baidcheck.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetBaid3209([FromBody] Models.v3209.BAIDRequest request)
    {
        Logger.LogInformation("Baid request: {Request}", request.Stringify());

        var commonResponse = await Mediator.Send(new BaidQuery(request.WechatQrStr));
        Models.v3209.BAIDResponse response;
        if (commonResponse.IsNewUser)
        {
            Logger.LogInformation("New user with access code {AccessCode}", request.WechatQrStr);

            response = new Models.v3209.BAIDResponse
            {
                Result = 1,
                PlayerType = 1,
                Baid = commonResponse.Baid,
            };

            return Ok(response);
        }

        response = Mappers.BaidResponseMapper.Map3209WithPostProcess(commonResponse);
        response.Result = 1;
        response.PlayerType = 0;
        response.IsDispAchievementTypeSet = true;
        response.IsDispSouuchiOn = true;
        
        return Ok(response);
    }
}