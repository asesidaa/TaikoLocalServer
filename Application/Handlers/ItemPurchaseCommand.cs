namespace TaikoLocalServer.Application.Handlers;

public readonly record struct ItemPurchaseCommand(
    uint Baid,
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
    public partial ValueTask<CommonItemPurchaseResponse> Handle(ItemPurchaseCommand request, CancellationToken cancellationToken);
}
