namespace TaikoLocalServer.Application.Handlers;

public partial class GetItemShopInfoQueryHandler
{
    public partial ValueTask<CommonItemShopInfoResponse> Handle(GetItemShopInfoQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var season = gameDataService.Green().ItemShopCatalog.ActiveSeason;
        if (season is null)
        {
            logger.LogInformation("Green GetItemShopInfo returning empty because item shop is disabled");
            return ValueTask.FromResult(new CommonItemShopInfoResponse { Result = 1 });
        }

        return ValueTask.FromResult(new CommonItemShopInfoResponse
        {
            Result = 1,
            VerupNo = season.VerupNo,
            SeasonId = season.SeasonId,
            Telop = season.Telop,
            StartDatetime = season.StartDatetime,
            EndDatetime = season.EndDatetime,
            AfterstartDays = season.AfterstartDays,
            BeforecloseDays = season.BeforecloseDays,
            AryItemshopData = season.Items
                .Select(item => new CommonItemShopInfoResponse.ItemShopData
                {
                    ItemNo = item.ItemNo,
                    ItemType = item.ItemType,
                    ItemId = item.ItemId,
                    ItemPrice = item.Price
                })
                .ToList()
        });
    }
}
