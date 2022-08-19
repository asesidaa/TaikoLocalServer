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

        var manager = MusicAttributeManager.Instance;
        var response = new CrownsDataResponse
        {
            Result = 1,
            CrownFlg = GZipBytesUtil.GetGZipBytes(new byte[manager.Musics.Count*4]),
            DondafulCrownFlg = GZipBytesUtil.GetGZipBytes(new byte[manager.Musics.Count*4])
        };

        return Ok(response);
    }
}