namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Controllers;

[ApiController]
[Route("/v09r02/chassis/baidcheck.php")]
public class BaidController : BaseProtocolController<BaidController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> Baid([FromBody] BAIDRequest request)
    {
        Logger.LogInformation("Yellow BAID request: {@Request}", request);
        var common = await Mediator.Send(new Ac15BaidQuery(GameEra.Yellow, request.AccessCode), HttpContext.RequestAborted);

        if (common.IsNewUser)
        {
            Logger.LogInformation("New Yellow user with access code {AccessCode}", request.AccessCode);

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
}
