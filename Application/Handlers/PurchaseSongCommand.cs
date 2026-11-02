using Throw;

namespace TaikoLocalServer.Application.Handlers;

public readonly record struct PurchaseSongCommand(uint Baid, GameEra Era, uint SongNo, uint Type, uint TokenId, uint Price) : IRequest<CommonSongPurchaseResponse>;

public readonly record struct PurchaseSongCommandCN(uint Baid, GameEra Era, uint SongNo, uint TokenId, uint Price) : IRequest<CommonSongPurchaseResponse>;

public partial class PurchaseSongCommandHandler(ITaikoDbContext context, ILogger<PurchaseSongCommandHandler> logger) 
    : IRequestHandler<PurchaseSongCommand, CommonSongPurchaseResponse>
{

    public ValueTask<CommonSongPurchaseResponse> Handle(PurchaseSongCommand request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
        GameEra.Green => HandleGreen(request, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
    };

    private partial ValueTask<CommonSongPurchaseResponse> HandleNijiiro(PurchaseSongCommand request, CancellationToken cancellationToken);
    private partial ValueTask<CommonSongPurchaseResponse> HandleGreen(PurchaseSongCommand request, CancellationToken cancellationToken);
}


public partial class PurchaseSongCommandHandlerCN(ITaikoDbContext context, ILogger<PurchaseSongCommandHandlerCN> logger) 
    : IRequestHandler<PurchaseSongCommandCN, CommonSongPurchaseResponse>
{

    public ValueTask<CommonSongPurchaseResponse> Handle(PurchaseSongCommandCN request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
        GameEra.Green => HandleGreen(request, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
    };

    private partial ValueTask<CommonSongPurchaseResponse> HandleNijiiro(PurchaseSongCommandCN request, CancellationToken cancellationToken);
    private partial ValueTask<CommonSongPurchaseResponse> HandleGreen(PurchaseSongCommandCN request, CancellationToken cancellationToken);
}
