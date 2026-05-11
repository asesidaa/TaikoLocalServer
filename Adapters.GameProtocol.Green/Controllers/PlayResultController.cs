namespace TaikoLocalServer.Adapters.GameProtocol.Green.Controllers;

[ApiController]
[Route("/v11r01/chassis/playresult.php")]
public class PlayResultController : BaseProtocolController<PlayResultController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> PlayResult([FromBody] PlayResultRequest request)
    {
        Logger.LogInformation(
            "Green PlayResult request: baid={Baid} chassis={ChassisId} payload_bytes={PayloadBytes}",
            request.BaidConf,
            request.ChassisIdConf,
            request.PlayresultData?.Length ?? 0);

        CommonPlayResultData commonRequest;
        try
        {
            commonRequest = PlayResultMappers.Map(
                Serializer.Deserialize<PlayResultDataRequest>(new ReadOnlySpan<byte>(request.PlayresultData ?? [])));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to deserialize Green PlayResultDataRequest for baid {Baid}", request.BaidConf);
            return Ok(new PlayResultResponse { Result = 0 });
        }

        var result = await Mediator.Send(
            new UpdatePlayResultCommand(request.BaidConf, GameEra.Green, commonRequest),
            HttpContext.RequestAborted);

        return Ok(PlayResultMappers.Map(result));
    }
}
