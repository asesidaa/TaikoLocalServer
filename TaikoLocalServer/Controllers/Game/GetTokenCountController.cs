using System.Text.Json;
using Throw;

namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r03/chassis/gettokencount.php")]
[ApiController]
public class GetTokenCountController : BaseController<GetTokenCountController>
{
    private readonly IUserDatumService userDatumService;

    public GetTokenCountController(IUserDatumService userDatumService)
    {
        this.userDatumService = userDatumService;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetTokenCount([FromBody] GetTokenCountRequest request)
    {
        Logger.LogInformation("GetTokenCount request : {Request}", request.Stringify());

        var user = await userDatumService.GetFirstUserDatumOrNull(request.Baid);
        user.ThrowIfNull($"User with baid {request.Baid} does not exist!");

        var tokenCountDict = new Dictionary<uint, int>();
        try
        {
            tokenCountDict = !string.IsNullOrEmpty(user.TokenCountDict)
                ? JsonSerializer.Deserialize<Dictionary<uint, int>>(user.TokenCountDict)
                : new Dictionary<uint, int>();
        }
        catch (JsonException e)
        {
            Logger.LogError(e, "Parsing TokenCountDict json data failed");
        }

        tokenCountDict.ThrowIfNull("TokenCountDict should never be null");

        if (!tokenCountDict.ContainsKey(4)) tokenCountDict.Add(4, 0);

        if (!tokenCountDict.ContainsKey(1000)) tokenCountDict.Add(1000, 0);

        var response = new GetTokenCountResponse
        {
            Result = 1
        };

        foreach (var (key, value) in tokenCountDict)
            response.AryTokenCountDatas.Add(new GetTokenCountResponse.TokenCountData
            {
                TokenCount = value,
                TokenId = key
            });

        return Ok(response);
    }
}