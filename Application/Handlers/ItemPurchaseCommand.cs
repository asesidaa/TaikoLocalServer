namespace TaikoLocalServer.Application.Handlers;

public readonly record struct ItemPurchaseCommand(
    uint Baid,
    GameEra Era,
    uint ItemNo,
    uint? ItemType,
    uint? ItemId,
    uint? ItemPrice
) : IRequest<CommonItemPurchaseResponse>;

public partial class ItemPurchaseCommandHandler(
    ITaikoDbContext context,
    IGameDataCatalog gameDataService,
    ILogger<ItemPurchaseCommandHandler> logger)
    : IRequestHandler<ItemPurchaseCommand, CommonItemPurchaseResponse>
{
    public ValueTask<CommonItemPurchaseResponse> Handle(ItemPurchaseCommand request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Green => HandleGreen(request, cancellationToken),
        GameEra.Blue => HandleBlue(request, cancellationToken),
        GameEra.Yellow => HandleYellow(request, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
    };

    private partial ValueTask<CommonItemPurchaseResponse> HandleGreen(ItemPurchaseCommand request, CancellationToken cancellationToken);
    private partial ValueTask<CommonItemPurchaseResponse> HandleBlue(ItemPurchaseCommand request, CancellationToken cancellationToken);
    private partial ValueTask<CommonItemPurchaseResponse> HandleYellow(ItemPurchaseCommand request, CancellationToken cancellationToken);

    private static bool CanAdd(uint current, uint delta)
        => delta <= uint.MaxValue - current;

    private static bool IsPreflight(ItemPurchaseCommand request)
        => request.ItemNo == 0
           && request.ItemType is null
           && request.ItemId is null
           && request.ItemPrice is null;
}
