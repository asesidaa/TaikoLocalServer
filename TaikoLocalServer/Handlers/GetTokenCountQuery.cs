using GameDatabase.Context;
using GameDatabase.Entities;
using TaikoLocalServer.Models.Application;

namespace TaikoLocalServer.Handlers;

public record GetTokenCountQuery(uint Baid) : IRequest<CommonGetTokenCountResponse>;

public class GetTokenCountQueryHandler(IGameDataService gameDataService, 
    TaikoDbContext context, 
    ILogger<GetTokenCountQueryHandler> logger)
    : IRequestHandler<GetTokenCountQuery, CommonGetTokenCountResponse>
{
    public async Task<CommonGetTokenCountResponse> Handle(GetTokenCountQuery request, CancellationToken cancellationToken)
    {
        var response = new CommonGetTokenCountResponse
        {
            Result = 1
        };
        
        string[] tokenNames = ["shopTokenId", "kaTokenId", "onePieceTokenId", "soshinaTokenId", "Yatsushika1TokenId", "Yatsushika2TokenId",
            "Yatsushika3TokenId", "Yatsushika4TokenId", "MaskedKid1TokenId", "MaskedKid2TokenId", "MaskedKid3TokenId", "MaskedKid4TokenId", 
            "Kiyoshi1TokenId", "Kiyoshi2TokenId", "Kiyoshi3TokenId", "Kiyoshi4TokenId", "Amitie1TokenId", "Amitie2TokenId", "Amitie3TokenId",
            "Amitie4TokenId", "Machina1TokenId", "Machina2TokenId", "Machina3TokenId", "Machina4TokenId"];
        
        var tokenDataDictionary = gameDataService.GetTokenDataDictionary();
        foreach (var tokenName in tokenNames)
        {
            tokenDataDictionary.TryGetValue(tokenName, out var tokenId);
            if (tokenId <= 0) continue;
            var token = await context.Tokens.FirstOrDefaultAsync(t => t.Baid == request.Baid &&
                                                                      t.Id   == tokenId,
                cancellationToken) ?? new Token
            {
                Id = tokenId,
                Count = 0
            };

            response.AryTokenCountDatas.Add(new CommonTokenCountData
            {
                TokenId = (uint)token.Id,
                TokenCount = token.Count
            });
            context.Tokens.Update(token);
        }

        await context.SaveChangesAsync(cancellationToken);
        return response;
    }
}