namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/getfolder.php")]
public class GetFolderController : BaseProtocolController<GetFolderController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetFolder([FromBody] GetfolderRequest request)
    {
        Logger.LogInformation("Blue GetFolder request: {Request}", request.Stringify());
        return Ok(new GetfolderResponse { Result = 1 });
    }
}
