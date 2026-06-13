using RedV08R00 = TaikoLocalServer.Adapters.GameProtocol.Red.Wire.V08R00;

namespace TaikoLocalServer.Adapters.GameProtocol.Red.Controllers;

[ApiController]
[Route("/v08r01/chassis/baidcheck.php")]
public class BaidController : BaseProtocolController<BaidController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> BaidCheck([FromBody] BAIDRequest request)
    {
        Logger.LogInformation("Red BAID request: {@Request}", request);
        var common = await Mediator.Send(new Ac15BaidQuery(GameEra.Red, request.AccessCode), HttpContext.RequestAborted);

        if (common.IsNewUser)
        {
            Logger.LogInformation("New Red user with access code {AccessCode}", request.AccessCode);

            return Ok(new BAIDResponse
            {
                Result = 1,
                PlayerType = 1,
                Baid = common.Baid
            });
        }

        var response = BaidResponseMapper.Map(common);
        response.AccessCode = request.AccessCode;
        response.IsPublish = true;
        response.PlayerType = 0;
        response.ComSvrResult = 1;
        response.Personid = "1";
        response.RegCountryId = "JPN";
        response.MbId = 1;
        response.PurposeId = 1;
        response.RegionId = 1;

        return Ok(response);
    }

    [HttpPost("/v08r00_tw/chassis/baidcheck.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> BaidCheckV08R00([FromBody] RedV08R00.BAIDRequest request)
    {
        Logger.LogInformation("Red v08r00 BAID request: {@Request}", request);
        var common = await Mediator.Send(new Ac15BaidQuery(GameEra.Red, request.AccessCode), HttpContext.RequestAborted);

        if (common.IsNewUser)
        {
            Logger.LogInformation("New Red v08r00 user with access code {AccessCode}", request.AccessCode);

            return Ok(new RedV08R00.BAIDResponse
            {
                Result = 1,
                PlayerType = 1,
                Baid = common.Baid
            });
        }

        var response = BaidResponseMapper.MapV08R00(common);
        response.AccessCode = request.AccessCode;
        response.IsPublish = true;
        response.PlayerType = 0;
        response.ComSvrResult = 1;
        response.Personid = "1";
        response.RegCountryId = "JPN";
        response.MbId = 1;
        response.PurposeId = 1;
        response.RegionId = 1;

        return Ok(response);
    }
}
