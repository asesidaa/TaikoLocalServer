using taiko.vsinterface;

namespace TaikoLocalServer.Controllers;

[ApiController]
[Route("/v12r03/chassis/initialdatacheck.php")]
public class InitialDataCheckController:ControllerBase
{
    private readonly ILogger<InitialDataCheckController> logger;

    public InitialDataCheckController(ILogger<InitialDataCheckController> logger)
    {
        this.logger = logger;
    }


    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult InitialDataCheck([FromBody] InitialdatacheckRequest request)
    {
        logger.LogInformation("InitialDataCheck request: {Request}", request.Stringify());
        var response = new InitialdatacheckResponse
        {
            Result = 1,
            IsDanplay = true,
            IsAibattle = true,
            IsClose = false
        };
        /*response.AryMovieInfoes.Add(new InitialdatacheckResponse.MovieData
        {
            MovieId = 2,
            EnableDays = 9999
        });*/
        
        response.AryTelopDatas.Add(new InitialdatacheckResponse.InformationData
        {
            InfoId = 0,
            VerupNo = 1
        });

        return Ok(response);
    }

}