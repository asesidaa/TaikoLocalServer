namespace TaikoLocalServer.Application.Handlers;

public partial class PurchaseSongCommandHandler
{
    private partial ValueTask<CommonSongPurchaseResponse> HandleGreen(PurchaseSongCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Green PurchaseSong stub for baid {Baid}, song {SongNo}, returning success", request.Baid, request.SongNo);
        return ValueTask.FromResult(new CommonSongPurchaseResponse { Result = 1 });
    }
}

public partial class PurchaseSongCommandHandlerCN
{
    private partial ValueTask<CommonSongPurchaseResponse> HandleGreen(PurchaseSongCommandCN request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Green PurchaseSongCN stub for baid {Baid}, song {SongNo}, returning success", request.Baid, request.SongNo);
        return ValueTask.FromResult(new CommonSongPurchaseResponse { Result = 1 });
    }
}
