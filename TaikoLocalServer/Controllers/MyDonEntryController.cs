using Microsoft.AspNetCore.Http;

namespace TaikoLocalServer.Controllers;

[Route("/v12r03/chassis/mydonentry.php")]
[ApiController]
public class MyDonEntryController : ControllerBase
{
    private readonly ILogger<MyDonEntryController> logger;
    public MyDonEntryController(ILogger<MyDonEntryController> logger) {
        this.logger = logger;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetMyDonEntry([FromBody] MydonEntryRequest request)
    {
        logger.LogInformation("MyDonEntry request : {Request}", request.Stringify());

        var response = new MydonEntryResponse
        {
            Result = 1,
            ComSvrResult = 1,
            AccessCode = request.AccessCode,
            Accesstoken = "12345",
            Baid = 1,
            MbId = 1,
            MydonName = request.MydonName,
            CardOwnNum = 1,
            IsPublish = true,
            RegCountryId = request.CountryId,
            PurposeId = 1,
            RegionId = 1
        };

        return Ok(response);
    }
}