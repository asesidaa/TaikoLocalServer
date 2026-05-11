namespace TaikoLocalServer.Application.Handlers;

public partial class GetTokenCountQueryHandler
{
    private partial async ValueTask<CommonGetTokenCountResponse> HandleNijiiro(GetTokenCountQuery request, CancellationToken cancellationToken)
    {
        var response = new CommonGetTokenCountResponse
        {
            Result = 1
        };

        string[] tokenNames = ["seasonTokenId", "eventTokenId", "onePieceTokenId", "soshinaTokenId", "Yatsushika1TokenId", "Yatsushika2TokenId",
            "Yatsushika3TokenId", "Yatsushika4TokenId", "MaskedKid1TokenId", "MaskedKid2TokenId", "MaskedKid3TokenId", "MaskedKid4TokenId",
            "Kiyoshi1TokenId", "Kiyoshi2TokenId", "Kiyoshi3TokenId", "Kiyoshi4TokenId", "Amitie1TokenId", "Amitie2TokenId", "Amitie3TokenId",
            "Amitie4TokenId", "Machina1TokenId", "Machina2TokenId", "Machina3TokenId", "Machina4TokenId"];

        var tokenDataDictionary = gameDataService.GetTokenDataDictionary();
        foreach (var tokenName in tokenNames)
        {
            tokenDataDictionary.TryGetValue(tokenName, out var tokenId);
            if (tokenId <= 0) continue;
            var token = await context.Tokens.FirstOrDefaultAsync(t => t.Baid == request.Baid &&
                                                                      t.Id == tokenId,
                cancellationToken);
            if (token is null)
            {
                token = new Token
                {
                    Baid = request.Baid,
                    Id = tokenId,
                    Count = 0,
                };
                context.Tokens.Add(token);
            }

            response.AryTokenCountDatas.Add(new CommonTokenCountData
            {
                TokenId = (uint)token.Id,
                TokenCount = token.Count
            });
        }

        await context.SaveChangesAsync(cancellationToken);
        return response;
    }
}
