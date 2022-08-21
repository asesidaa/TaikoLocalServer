using System.Collections;
using System.Collections.Specialized;
using Microsoft.AspNetCore.Http;
using TaikoLocalServer.Common;
using TaikoLocalServer.Common.Enums;
using TaikoLocalServer.Utils;

namespace TaikoLocalServer.Controllers;

[Route("/v12r03/chassis/crownsdata.php")]
[ApiController]
public class CrownsDataController : ControllerBase
{
    private readonly ILogger<CrownsDataController> logger;
    
    private readonly IConfiguration configuration;
    
    public CrownsDataController(ILogger<CrownsDataController> logger, IConfiguration configuration)
    {
        this.logger = logger;
        this.configuration = configuration;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult CrownsData([FromBody] CrownsDataRequest request)
    {
        logger.LogInformation("CrownsData request : {Request}", request.Stringify());

        var crown = new ushort[Constants.CROWN_FLAG_ARRAY_SIZE];
        var bytes = new byte[Constants.DONDAFUL_CROWN_FLAG_ARRAY_SIZE];
        
        var response = new CrownsDataResponse
        {
            Result = 1,
            CrownFlg = GZipBytesUtil.GetGZipBytes(crown),
            DondafulCrownFlg = GZipBytesUtil.GetGZipBytes(bytes)
        };

        return Ok(response);
    }
}