namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Controllers;

using FinalWire = TaikoLocalServer.Adapters.GameProtocol.Kimidori.FinalWire;

[ApiController]
public sealed class StartupVersionController : BaseProtocolController<StartupVersionController>
{
    [HttpPost(KimidoriRoutePrefixes.Final + "/startupauth.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> StartupAuth([FromBody] FinalWire.StartupAuthRequest request)
    {
        Logger.LogInformation("Kimidori final StartupAuth request: {@Request}", request);

        var response = new FinalWire.StartupAuthResponse { Result = 1 };
        var movieData = await Mediator.Send(
            new GetStartupMovieDataQuery(request.HddVer),
            HttpContext.RequestAborted);
        response.AryMovieInfoes.AddRange(movieData.Select(movie =>
            new FinalWire.StartupAuthResponse.MovieData
            {
                MovieId = movie.MovieId,
                EnableDays = movie.EnableDays
            }));

        response.AryOperationInfoes.AddRange(request.AryOperationInfoes.Select(input =>
            new FinalWire.StartupAuthResponse.OperationData
            {
                KeyData = input.KeyData,
                ValueData = input.ValueData
            }));

        return Ok(response);
    }

    [HttpPost(KimidoriRoutePrefixes.Final + "/verupauth.php")]
    [Produces("application/protobuf")]
    public IActionResult VerupAuth([FromBody] FinalWire.VerupAuthRequest request)
    {
        Logger.LogInformation("Kimidori final VerupAuth request: {@Request}", request);
        return Ok(new FinalWire.VerupAuthResponse { Result = 1 });
    }

    [HttpPost(KimidoriRoutePrefixes.Final + "/verupcomplete.php")]
    [Produces("application/protobuf")]
    public IActionResult VerupComplete([FromBody] FinalWire.VerupCompleteRequest request)
    {
        Logger.LogInformation("Kimidori final VerupComplete request: {@Request}", request);
        return Ok(new FinalWire.VerupCompleteResponse { Result = 1 });
    }
}
