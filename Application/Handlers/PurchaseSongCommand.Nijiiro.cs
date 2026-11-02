using Throw;

namespace TaikoLocalServer.Application.Handlers;

public partial class PurchaseSongCommandHandler
{
    private partial async ValueTask<CommonSongPurchaseResponse> HandleNijiiro(PurchaseSongCommand request, CancellationToken cancellationToken)
    {
        var user = await context.UserData
            .Include(u => u.Tokens)
            .FirstOrDefaultAsync(u => u.Baid == request.Baid, cancellationToken);
        user.ThrowIfNull($"User with baid {request.Baid} does not exist!");
        var saveData = await context.GetOrCreateNijiiroSaveDataAsync(request.Baid, cancellationToken);
        
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

        if (request.Type == 1)
        {
            if (saveData.UnlockedUraSongIdList.Contains(request.SongNo))
            {
                logger.LogWarning("User with baid {Baid} already has song with id {SongNo} unlocked!", request.Baid, request.SongNo);
                return new CommonSongPurchaseResponse { Result = 0 };
            }
            
            saveData.UnlockedUraSongIdList.Add(request.SongNo);
        }
        else
        {
            if (saveData.UnlockedSongIdList.Contains(request.SongNo))
            {
                logger.LogWarning("User with baid {Baid} already has song with id {SongNo} unlocked!", request.Baid, request.SongNo);
                return new CommonSongPurchaseResponse { Result = 0 };
            }
            
            saveData.UnlockedSongIdList.Add(request.SongNo);
        }
        
        await context.SaveChangesAsync(cancellationToken);
        return new CommonSongPurchaseResponse { Result = 1, TokenCount = token.Count };
    }
}

public partial class PurchaseSongCommandHandlerCN
{
    private partial async ValueTask<CommonSongPurchaseResponse> HandleNijiiro(PurchaseSongCommandCN request, CancellationToken cancellationToken)
    {
        var user = await context.UserData
            .Include(u => u.Tokens)
            .FirstOrDefaultAsync(u => u.Baid == request.Baid, cancellationToken);
        user.ThrowIfNull($"User with baid {request.Baid} does not exist!");
        var saveData = await context.GetOrCreateNijiiroSaveDataAsync(request.Baid, cancellationToken);
        if (saveData.UnlockedSongIdList.Contains(request.SongNo))
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
        
        saveData.UnlockedSongIdList.Add(request.SongNo);
        
        await context.SaveChangesAsync(cancellationToken);
        return new CommonSongPurchaseResponse { Result = 1, TokenCount = token.Count };
    }
}
