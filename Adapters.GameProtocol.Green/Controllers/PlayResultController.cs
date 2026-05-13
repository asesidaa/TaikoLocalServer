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
            "Green PlayResult request: baid={Baid} chassis={ChassisId} payload_bytes={PayloadBytes} payload_hex={PayloadHex}",
            request.BaidConf,
            request.ChassisIdConf,
            request.PlayresultData?.Length ?? 0,
            GreenPlayResultPayloadDecoder.HexPreview(request.PlayresultData ?? []));

        CommonPlayResultData commonRequest;
        try
        {
            var decoded = GreenPlayResultPayloadDecoder.Decode(request.PlayresultData ?? []);
            Logger.LogInformation(
                "Green PlayResult payload decoded as {Format}, decoded_bytes={DecodedBytes}",
                decoded.Format,
                decoded.DecodedBytes);
            commonRequest = PlayResultMappers.Map(decoded.Request);
            if (commonRequest.Baid != 0 && commonRequest.Baid != request.BaidConf)
            {
                Logger.LogWarning(
                    "Rejecting Green PlayResult baid mismatch: outer={OuterBaid}, inner={InnerBaid}",
                    request.BaidConf,
                    commonRequest.Baid);
                return Ok(new PlayResultResponse { Result = 0 });
            }

            Logger.LogInformation(
                "Green PlayResult received dump:{NewLine}{Dump}",
                Environment.NewLine,
                GreenPlayResultPayloadDecoder.BuildReceivedDump(decoded.Request, commonRequest));
        }
        catch (GreenPlayResultPayloadDecodeException ex)
        {
            Logger.LogError(
                ex,
                "Failed to decode gzip-compressed Green PlayResultDataRequest for baid {Baid}",
                request.BaidConf);
            return Ok(new PlayResultResponse { Result = 0 });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to process Green PlayResultDataRequest for baid {Baid}", request.BaidConf);
            return Ok(new PlayResultResponse { Result = 0 });
        }

        var result = await Mediator.Send(
            new UpdatePlayResultCommand(request.BaidConf, GameEra.Green, commonRequest),
            HttpContext.RequestAborted);

        return Ok(PlayResultMappers.Map(result));
    }
}
