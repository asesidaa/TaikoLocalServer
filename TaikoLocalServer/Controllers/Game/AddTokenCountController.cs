using System.Text.Json;
using Throw;

namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r03/chassis/addtokencount.php")]
[ApiController]
public class AddTokenCountController : BaseController<AddTokenCountController>
{
    private readonly IUserDatumService userDatumService;

    public AddTokenCountController(IUserDatumService userDatumService)
    {
        this.userDatumService = userDatumService;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> AddTokenCount([FromBody] AddTokenCountRequest request)
    {
        Logger.LogInformation("AddTokenCount request : {Request}", request.Stringify());

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

        foreach (var addTokenCountData in request.AryAddTokenCountDatas)
        {
            var tokenId = addTokenCountData.TokenId;
            var addTokenCount = addTokenCountData.AddTokenCount;

            if (tokenCountDict.ContainsKey(tokenId))
                tokenCountDict[tokenId] += addTokenCount;
            else
                tokenCountDict.Add(tokenId, addTokenCount);
        }

        user.TokenCountDict = JsonSerializer.Serialize(tokenCountDict);
        await userDatumService.UpdateUserDatum(user);

        var response = new AddTokenCountResponse
        {
            Result = 1
        };

        return Ok(response);
    }
}