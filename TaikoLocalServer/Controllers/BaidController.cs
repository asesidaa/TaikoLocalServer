namespace TaikoLocalServer.Controllers;

[ApiController]
[Route("/v12r03/chassis/baidcheck.php")]
public class BaidController:ControllerBase
{
    private readonly ILogger<BaidController> logger;

    public BaidController(ILogger<BaidController> logger)
    {
        this.logger = logger;
    }


    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult InitialDataCheck([FromBody] BAIDRequest request)
    {
        logger.LogInformation("Baid request: {Request}", request.Stringify());
        var response = new BAIDResponse
        {
            Result = 1,
            ComSvrResult = 1,
            IsPublish = true,
            AccessCode = request.AccessCode,
            Accesstoken = "123456",
            RegCountryId = "JPN",
            Baid = 1,
            MbId = 1,
            PlayerType = 2
        };

        return Ok(response);
    }

}