using TaikoLocalServer.Adapters.GameProtocol.Shared.Wire;

namespace TaikoLocalServer.Adapters.GameProtocol.Shared.Controllers;

[ApiController]
[Route("/v01r00/chassis/startupauth.php")]
public sealed class StartupAuthController : BaseProtocolController<StartupAuthController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> StartupAuth([FromBody] StartupAuthRequest request)
    {
        Logger.LogInformation(
            "StartupAuth request: {Request}", request.Stringify());

        var response = new StartupAuthResponse { Result = 1 };
        var movieData = await Mediator.Send(
            new GetStartupMovieDataQuery(request.HddVer),
            HttpContext.RequestAborted);
        response.AryMovieInfoes.AddRange(movieData.Select(movie =>
            new StartupAuthResponse.MovieData
            {
                MovieId = movie.MovieId,
                EnableDays = movie.EnableDays
            }));

        response.AryOperationInfoes.AddRange(request.AryOperationInfoes.Select(input =>
            new StartupAuthResponse.OperationData
            {
                KeyData = input.KeyData,
                ValueData = input.ValueData
            }));

        return Ok(response);
    }
}
