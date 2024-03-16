using GameDatabase.Context;
using TaikoLocalServer.Models.Application;
using Throw;

namespace TaikoLocalServer.Handlers;

public record PurchaseSongCommand(uint Baid, uint SongNo, uint TokenId, uint Price) : IRequest<CommonSongPurchaseResponse>;

public class PurchaseSongCommandHandler(TaikoDbContext context, ILogger<PurchaseSongCommandHandler> logger) 
    : IRequestHandler<PurchaseSongCommand, CommonSongPurchaseResponse>
{

    public async Task<CommonSongPurchaseResponse> Handle(PurchaseSongCommand request, CancellationToken cancellationToken)
    {
        var user = await context.UserData
            .Include(u => u.Tokens)
            .FirstOrDefaultAsync(u => u.Baid == request.Baid, cancellationToken);
        user.ThrowIfNull($"User with baid {request.Baid} does not exist!");
        if (user.UnlockedSongIdList.Contains(request.SongNo))
        {
            logger.LogWarning("User with baid {Baid} already has song with id {SongNo} unlocked!", request.Baid, request.SongNo);
            return new CommonSongPurchaseResponse { Result = 0 };
        }
        
        var token = user.Tokens.FirstOrDefault(t => t.Id == request.TokenId);
        if (token is not null && token.Count >= request.Price)
        {
            token.Count -= (int)request.Price;
        }
        else
        {
            logger.LogError("User with baid {Baid} does not have enough tokens to purchase song with id {SongNo}!", request.Baid, request.SongNo);
            return new CommonSongPurchaseResponse { Result = 0 };
        }
        
        user.UnlockedSongIdList.Add(request.SongNo);
        context.UserData.Update(user);
        await context.SaveChangesAsync(cancellationToken);
        return new CommonSongPurchaseResponse { Result = 1, TokenCount = token.Count };
    }
}