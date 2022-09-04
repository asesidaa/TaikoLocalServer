namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r03/chassis/addtokencount.php")]
[ApiController]
public class AddTokenCountController : BaseController<AddTokenCountController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult AddTokenCount([FromBody] AddTokenCountRequest request)
    {
        Logger.LogInformation("AddTokenCount request : {Request}", request.Stringify());

        var response = new AddTokenCountResponse
        {
            Result = 1
        };

        return Ok(response);
    }
}