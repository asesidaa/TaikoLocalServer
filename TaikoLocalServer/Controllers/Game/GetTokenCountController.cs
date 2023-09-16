using System.Text.Json;
using Throw;

namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r00_cn/chassis/gettokencount.php")]
[ApiController]
public class GetTokenCountController : BaseController<GetTokenCountController>
{
    private readonly IGameDataService gameDataService;
    private readonly IUserDatumService userDatumService;

    public GetTokenCountController(IUserDatumService userDatumService, IGameDataService gameDataService)
    {
        this.userDatumService = userDatumService;
        this.gameDataService = gameDataService;
    }
    
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> GetTokenCount([FromBody] GetTokenCountRequest request)
    {
        Logger.LogInformation("GetTokenCount request : {Request}", request.Stringify());

        var user = await userDatumService.GetFirstUserDatumOrNull(request.Baid);
        var tokenDataDictionary = gameDataService.GetTokenDataDictionary();
        tokenDataDictionary.TryGetValue("shopTokenId", out var shopTokenId);
        tokenDataDictionary.TryGetValue("kaTokenId", out var kaTokenId);
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
            Logger.LogError(e, "Parsing TokenCountDict data for user with baid {Request} failed!", request.Baid);
        }

        tokenCountDict.ThrowIfNull("TokenCountDict should never be null");

        if (tokenCountDict.Count == 0) tokenCountDict.Add(shopTokenId, 0);

        tokenCountDict.TryAdd(shopTokenId, 0);

        tokenCountDict.TryAdd(kaTokenId, 0);

        user.TokenCountDict = JsonSerializer.Serialize(tokenCountDict);
        await userDatumService.UpdateUserDatum(user);

        var response = new GetTokenCountResponse
        {
            Result = 1
        };
        
        response.AryTokenCountDatas.Add(new GetTokenCountResponse.TokenCountData
        {
            TokenCount = tokenCountDict[shopTokenId],
            TokenId = shopTokenId
        });

        response.AryTokenCountDatas.Add(new GetTokenCountResponse.TokenCountData
        {
            TokenCount = tokenCountDict[kaTokenId],
            TokenId = kaTokenId
        });
        
        return Ok(response);
    }
}