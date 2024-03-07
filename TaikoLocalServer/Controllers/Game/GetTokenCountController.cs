using System.Text.Json;
using Throw;

namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r08_ww/chassis/gettokencount_iut9g23g.php")]
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
        tokenDataDictionary.TryGetValue("onePieceTokenId", out var onePieceTokenId);
        tokenDataDictionary.TryGetValue("soshinaTokenId", out var soshinaTokenId);
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

        var response = new GetTokenCountResponse
        {
            Result = 1
        };

        if (tokenCountDict.Count == 0) tokenCountDict.Add(1, 0);
        if (shopTokenId > 0)
        {
            var castedShopTokenId = (uint)shopTokenId;
            tokenCountDict.TryAdd(castedShopTokenId, 0);
            response.AryTokenCountDatas.Add(new GetTokenCountResponse.TokenCountData
            {
                TokenCount = tokenCountDict[castedShopTokenId],
                TokenId = castedShopTokenId
            });
        }

        if (kaTokenId > 0)
        {
            var castedKaTokenId = (uint)kaTokenId;
            tokenCountDict.TryAdd(castedKaTokenId, 0);
            response.AryTokenCountDatas.Add(new GetTokenCountResponse.TokenCountData
            {
                TokenCount = tokenCountDict[castedKaTokenId],
                TokenId = castedKaTokenId
            });
        }

        if (onePieceTokenId > 0)
        {
            var castedOnePieceTokenId = (uint)onePieceTokenId;
            tokenCountDict.TryAdd(castedOnePieceTokenId, 0);
            response.AryTokenCountDatas.Add(new GetTokenCountResponse.TokenCountData
            {
                TokenCount = tokenCountDict[castedOnePieceTokenId],
                TokenId = castedOnePieceTokenId
            });
        }

        if (soshinaTokenId > 0)
        {
            var castedSoshinaTokenId = (uint)soshinaTokenId;
            tokenCountDict.TryAdd(castedSoshinaTokenId, 0);
            response.AryTokenCountDatas.Add(new GetTokenCountResponse.TokenCountData
            {
                TokenCount = tokenCountDict[castedSoshinaTokenId],
                TokenId = castedSoshinaTokenId
            });
        }

        user.TokenCountDict = JsonSerializer.Serialize(tokenCountDict);
        await userDatumService.UpdateUserDatum(user);

        return Ok(response);
    }
}