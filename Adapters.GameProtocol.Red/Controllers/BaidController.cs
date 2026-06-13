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
        var common = await Mediator.Send(new BaidQuery(GameEra.Red, request.AccessCode), HttpContext.RequestAborted);

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
}
