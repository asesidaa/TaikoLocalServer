namespace TaikoLocalServer.Application.Handlers;

public partial class ItemPurchaseCommandHandler
{
    public partial async ValueTask<CommonItemPurchaseResponse> Handle(
        ItemPurchaseCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Applying Green item purchase for baid {Baid}, item {ItemNo}", request.Baid, request.ItemNo);
        var saveData = await context.GetOrCreateGreenSaveDataAsync(request.Baid, cancellationToken);
        var green = gameDataService.Green();
        var available = saveData.TotalGetDonmedal >= saveData.TotalUseDonmedal
            ? saveData.TotalGetDonmedal - saveData.TotalUseDonmedal
            : 0;

        if (!green.ItemShop.TryGetValue(request.ItemNo, out var item)
            || request.ItemType != item.ItemType
            || request.ItemId != item.ItemId
            || request.ItemPrice != item.Price
            || item.Price == 0
            || item.Price > available)
        {
            return new CommonItemPurchaseResponse
            {
                Result = 0,
                TotalGetDonmedal = saveData.TotalGetDonmedal,
                TotalUseDonmedal = saveData.TotalUseDonmedal
            };
        }

        saveData.TotalUseDonmedal += item.Price;

        await context.SaveChangesAsync(cancellationToken);
        return new CommonItemPurchaseResponse
        {
            Result = 1,
            TotalGetDonmedal = saveData.TotalGetDonmedal,
            TotalUseDonmedal = saveData.TotalUseDonmedal
        };
    }
}
