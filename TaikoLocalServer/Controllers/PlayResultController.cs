using Microsoft.AspNetCore.Http;
using ProtoBuf;

namespace TaikoLocalServer.Controllers;

[Route("/v12r03/chassis/playresult.php")]
[ApiController]
public class PlayResultController : ControllerBase
{
    private readonly ILogger<PlayResultController> logger;
    public PlayResultController(ILogger<PlayResultController> logger) {
        this.logger = logger;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult UploadPlayResult([FromBody] PlayResultRequest request)
    {
        logger.LogInformation("PlayResultController request : {Request}", request.Stringify());

        var playResultData = Serializer.Deserialize<PlayResultDataRequest>(new ReadOnlyMemory<byte>(request.PlayresultData));
        
        logger.LogInformation("Play result data {Data}", playResultData.Stringify());

        var response = new PlayResultResponse()
        {
            Result = 1
        };

        return Ok(response);
    }
}