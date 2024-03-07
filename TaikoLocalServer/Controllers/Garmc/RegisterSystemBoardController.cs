using Garm;

namespace TaikoLocalServer.Controllers.Garmc;

[Route("/v1/s12-jp-dev/garm.SystemBoard/RegisterSystemBoard")]
[ApiController]
public class RegisterSystemBoardController : BaseController<RegisterSystemBoardController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<RegisterSystemBoardResponse> RegisterSystemBoard()
    {
        HttpContext.Request.EnableBuffering();
        var body = await HttpContext.Request.BodyReader.ReadAsync();
        var request = Serializer.Deserialize<RegisterSystemBoardRequest>(body.Buffer);
        Logger.LogInformation("RegisterSystemBoard request: {Request}", request.Stringify());
        var response = new RegisterSystemBoardResponse
        {
            Token = "114514"
        };
        Response.Headers.Append("x-drpc-code", "0");
        return response;
    }
}