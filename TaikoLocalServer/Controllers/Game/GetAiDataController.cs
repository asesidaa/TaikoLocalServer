using Throw;

namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r03/chassis/getaidata.php")]
[ApiController]
public class GetAiDataController : BaseController<GetAiDataController>
{
    private readonly IUserDatumService userDatumService;

    public GetAiDataController(IUserDatumService userDatumService)
    {
        this.userDatumService = userDatumService;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetAiData([FromBody] GetAiDataRequest request)
    {
        Logger.LogInformation("GetAiData request : {Request}", request.Stringify());

        var user = await userDatumService.GetFirstUserDatumOrNull(request.Baid);
        user.ThrowIfNull($"User with baid {request.Baid} does not exist!");
        var response = new GetAiDataResponse
        {
            Result = 1,
            TotalWinnings = (uint)user.AiWinCount,
            InputMedian = "1000",
            InputVariance = "2000"
        };

        return Ok(response);
    }
}