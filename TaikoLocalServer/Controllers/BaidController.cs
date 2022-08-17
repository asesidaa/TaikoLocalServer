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
            PlayerType = 2,
            AryCostumedata = new BAIDResponse.CostumeData
            {
                Costume1 = 1,
                Costume2 = 1,
                Costume3 = 1,
                Costume4 = 1,
                Costume5 = 1
            },
            DefaultToneSetting = 0,
            AryCrownCounts = new uint[] {0,0,0},
            AryScoreRankCounts = new uint[]
            {
                0,0,0,0,0,0,0
            }
        };

        return Ok(response);
    }

}