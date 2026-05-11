namespace TaikoLocalServer.Application.Handlers;

public partial class ItemPurchaseCommandHandler
{
    public partial ValueTask<CommonItemPurchaseResponse> Handle(ItemPurchaseCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Green ItemPurchase stub for baid {Baid}, item {ItemNo}, returning success", request.Baid, request.ItemNo);
        return ValueTask.FromResult(new CommonItemPurchaseResponse());
    }
}
