namespace TaikoLocalServer.Application.Handlers;

public partial class GetItemShopInfoQueryHandler
{
    public partial ValueTask<CommonItemShopInfoResponse> Handle(GetItemShopInfoQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Green GetItemShopInfo stub returning empty");
        return ValueTask.FromResult(new CommonItemShopInfoResponse());
    }
}
