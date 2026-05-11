namespace TaikoLocalServer.Application.Handlers;

public partial class ItemPurchaseCommandHandler
{
    public partial async ValueTask<CommonItemPurchaseResponse> Handle(
        ItemPurchaseCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Applying Green item purchase for baid {Baid}, item {ItemNo}", request.Baid, request.ItemNo);
        var saveData = await context.GetOrCreateGreenSaveDataAsync(request.Baid, cancellationToken);
        var price = request.ItemPrice.GetValueOrDefault();
        if (price > 0 && saveData.TotalGetDonmedal >= saveData.TotalUseDonmedal + price)
        {
            saveData.TotalUseDonmedal += price;
        }

        await context.SaveChangesAsync(cancellationToken);
        return new CommonItemPurchaseResponse
        {
            Result = 1,
            TotalGetDonmedal = saveData.TotalGetDonmedal,
            TotalUseDonmedal = saveData.TotalUseDonmedal
        };
    }
}
