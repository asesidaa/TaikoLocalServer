namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r08_ww/chassis/setanystring_mssxf3bo.php")]
[ApiController]
public class SetAnyStringController : BaseController<SetAnyStringController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult SetAnyString([FromBody] SetAnyStringRequest request)
    {
        Logger.LogInformation("SetAnyString request : {Request}", request.Stringify());

        var response = new SetAnyStringResponse
        {
            Result = 1,
        };

        return Ok(response);
    }
}