namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/baidcheck.php")]
public class BaidController : BaseProtocolController<BaidController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> Baid([FromBody] BAIDRequest request)
    {
        Logger.LogInformation("Blue BAID request: {Request}", request.Stringify());
        var common = await Mediator.Send(new BaidQuery(GameEra.Blue, request.AccessCode), HttpContext.RequestAborted);

        if (common.IsNewUser)
        {
            Logger.LogInformation("New Blue user with access code {AccessCode}", request.AccessCode);

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

        return Ok(response);
    }
}
