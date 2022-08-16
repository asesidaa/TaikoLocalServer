using Microsoft.AspNetCore.Http;
using TaikoLocalServer.Utils;

namespace TaikoLocalServer.Controllers;

[Route("/v12r03/chassis/crownsdata.php")]
[ApiController]
public class CrownsDataController : ControllerBase
{
    private readonly ILogger<CrownsDataController> logger;
    public CrownsDataController(ILogger<CrownsDataController> logger) {
        this.logger = logger;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult CrownsData([FromBody] CrownsDataRequest request)
    {
        logger.LogInformation("CrownsData request : {Request}", request.Stringify());

        var response = new CrownsDataResponse
        {
            Result = 1,
            CrownFlg = GZipBytesUtil.GetEmptyJsonGZipBytes(),
            DondafulCrownFlg = GZipBytesUtil.GetEmptyJsonGZipBytes()
        };

        return Ok(response);
    }
}