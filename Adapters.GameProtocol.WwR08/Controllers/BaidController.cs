namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Controllers;

[ApiController]
public class BaidController : BaseProtocolController<BaidController>
{
    [HttpPost("/v12r08_ww/chassis/baidcheck_dcfxit1u.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetBaid([FromBody] BAIDRequest request)
    {
        Logger.LogInformation("Baid request: {Request}", request.Stringify());
        var commonResponse = await Mediator.Send(new BaidQuery(GameEra.Nijiiro, request.AccessCode), HttpContext.RequestAborted);
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

        response = BaidResponseMapper.MapWW08WithPostProcess(commonResponse);
        response.PlayerType = 0;

        return Ok(response);
    }
}
