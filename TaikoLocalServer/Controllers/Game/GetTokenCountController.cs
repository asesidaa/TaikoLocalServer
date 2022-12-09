namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r03/chassis/gettokencount.php")]
[ApiController]
public class GetTokenCountController : BaseController<GetTokenCountController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetTokenCount([FromBody] GetTokenCountRequest request)
    {
        Logger.LogInformation("GetTokenCount request : {Request}", request.Stringify());

        var response = new GetTokenCountResponse
        {
            Result = 1
        };

        response.AryTokenCountDatas.Add(new GetTokenCountResponse.TokenCountData
        {
            TokenCount = 10,
            TokenId = 1
        });

        return Ok(response);
    }
}