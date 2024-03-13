using System.Text.Json;
using SharedProject.Entities;
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
        user.ThrowIfNull($"User with baid {request.Baid} does not exist!");

        var tokenDataDictionary = gameDataService.GetTokenDataDictionary();
        tokenDataDictionary.TryGetValue("shopTokenId", out var shopTokenId);
        tokenDataDictionary.TryGetValue("kaTokenId", out var kaTokenId);
        tokenDataDictionary.TryGetValue("onePieceTokenId", out var onePieceTokenId);
        tokenDataDictionary.TryGetValue("soshinaTokenId", out var soshinaTokenId);
        tokenDataDictionary.TryGetValue("Yatsushika1TokenId", out var yatsushika1TokenId);
        tokenDataDictionary.TryGetValue("Yatsushika2TokenId", out var yatsushika2TokenId);
        tokenDataDictionary.TryGetValue("Yatsushika3TokenId", out var yatsushika3TokenId);
        tokenDataDictionary.TryGetValue("Yatsushika4TokenId", out var yatsushika4TokenId);
        tokenDataDictionary.TryGetValue("MaskedKid1TokenId", out var maskedKid1TokenId);
        tokenDataDictionary.TryGetValue("MaskedKid2TokenId", out var maskedKid2TokenId);
        tokenDataDictionary.TryGetValue("MaskedKid3TokenId", out var maskedKid3TokenId);
        tokenDataDictionary.TryGetValue("MaskedKid4TokenId", out var maskedKid4TokenId);
        tokenDataDictionary.TryGetValue("Kiyoshi1TokenId", out var kiyoshi1TokenId);
        tokenDataDictionary.TryGetValue("Kiyoshi2TokenId", out var kiyoshi2TokenId);
        tokenDataDictionary.TryGetValue("Kiyoshi3TokenId", out var kiyoshi3TokenId);
        tokenDataDictionary.TryGetValue("Kiyoshi4TokenId", out var kiyoshi4TokenId);
        tokenDataDictionary.TryGetValue("Amitie1TokenId", out var amitie1TokenId);
        tokenDataDictionary.TryGetValue("Amitie2TokenId", out var amitie2TokenId);
        tokenDataDictionary.TryGetValue("Amitie3TokenId", out var amitie3TokenId);
        tokenDataDictionary.TryGetValue("Amitie4TokenId", out var amitie4TokenId);
        tokenDataDictionary.TryGetValue("Machina1TokenId", out var machina1TokenId);
        tokenDataDictionary.TryGetValue("Machina2TokenId", out var machina2TokenId);
        tokenDataDictionary.TryGetValue("Machina3TokenId", out var machina3TokenId);
        tokenDataDictionary.TryGetValue("Machina4TokenId", out var machina4TokenId);

        int[] tokenDataIdArray =
        {
            shopTokenId, kaTokenId, onePieceTokenId, soshinaTokenId, yatsushika1TokenId, yatsushika2TokenId,
            yatsushika3TokenId, yatsushika4TokenId, maskedKid1TokenId, maskedKid2TokenId, maskedKid3TokenId,
            maskedKid4TokenId, kiyoshi1TokenId, kiyoshi2TokenId, kiyoshi3TokenId, kiyoshi4TokenId, amitie1TokenId,
            amitie2TokenId, amitie3TokenId, amitie4TokenId, machina1TokenId, machina2TokenId, machina3TokenId,
            machina4TokenId
        };

        var response = new GetTokenCountResponse
        {
            Result = 1
        };

        foreach (var tokenDataId in tokenDataIdArray)
        {
            if (tokenDataId <= 0) continue;
            var castedTokenDataId = (uint)tokenDataId;
            if (user.Tokens.All(token => token.Id != castedTokenDataId))
            {
                user.Tokens.Add(new Token
                {
                    Id = (int)castedTokenDataId,
                    Count = 0
                }); 
            }
            var tokenCount = user.Tokens.First(token => token.Id == castedTokenDataId).Count;
            response.AryTokenCountDatas.Add(new GetTokenCountResponse.TokenCountData
            {
                TokenCount = tokenCount,
                TokenId = castedTokenDataId
            });
        }
        
        await userDatumService.UpdateUserDatum(user);

        return Ok(response);
    }
}