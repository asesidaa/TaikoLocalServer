using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Adapters.GameProtocol.Yellow.Controllers;
using TaikoLocalServer.Adapters.GameProtocol.Yellow.Wire;
using TaikoLocalServer.Tests.Blue;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowBanacoinCompatibilityTests
{
    [Fact]
    public void YellowBanacoinControllers_ReturnMinimalStatelessSuccessResponses()
    {
        var balance = ((OkObjectResult)CreateController<BalanceCheckController>().BalanceCheck(new BalancecheckRequest
        {
            ChassisId = "YCHASSIS",
            ShopId = "YSHOP",
            Personid = "YPERSON"
        })).Value as BalancecheckResponse;
        Assert.NotNull(balance);
        Assert.Equal(1u, balance!.Result);
        Assert.Equal("YPERSON", balance.Personid);
        Assert.False(balance.ShouldSerializeBnidResult());
        Assert.False(balance.ShouldSerializeCoinCoupon());

        var payment = ((OkObjectResult)CreateController<BanacoinPaymentController>().BanacoinPayment(new BanacoinpaymentRequest
        {
            ChassisId = "YCHASSIS",
            ShopId = "YSHOP",
            ShopName = "YSHOPNAME",
            Personid = "YPERSON",
            Mode = 3,
            BanacoinPrice = 200
        })).Value as BanacoinpaymentResponse;
        Assert.NotNull(payment);
        Assert.Equal(1u, payment!.Result);
        Assert.Equal("YPERSON", payment.Personid);
        Assert.False(payment.ShouldSerializeBnidResult());
        Assert.False(payment.ShouldSerializeChid());

        var error = ((OkObjectResult)CreateController<BanacoinErrorLogController>().BanacoinErrorLog(new BanacoinerrorlogRequest
        {
            ChassisId = "YCHASSIS",
            ShopId = "YSHOP",
            ShopName = "YSHOPNAME",
            Personid = "YPERSON",
            TimeoutDatetime = "20260608160000"
        })).Value as BanacoinerrorlogResponse;
        Assert.NotNull(error);
        Assert.Equal(1u, error!.Result);

        var info = ((OkObjectResult)CreateController<GetBanacoinInfoController>().GetBanacoinInfo(new GetbanacoininfoRequest
        {
            DeviceType = 1,
            AccessCode = "YACCESS",
            ChipId = "YCHIP",
            ChassisId = "YCHASSIS",
            ShopId = "YSHOP",
            CountryId = "JPN"
        })).Value as GetbanacoininfoResponse;
        Assert.NotNull(info);
        Assert.Equal(1u, info!.Result);
        Assert.False(info.ShouldSerializePlayerType());
        Assert.False(info.ShouldSerializeComSvrResult());
        Assert.False(info.ShouldSerializeMbId());
        Assert.False(info.ShouldSerializeBaid());
        Assert.False(info.ShouldSerializeAccessCode());
        Assert.False(info.ShouldSerializeIsPublish());
        Assert.False(info.ShouldSerializeCardOwnNum());
        Assert.False(info.ShouldSerializeRegCountryId());
        Assert.False(info.ShouldSerializePurposeId());
        Assert.False(info.ShouldSerializeRegionId());
        Assert.False(info.ShouldSerializePersonid());
    }

    [Fact]
    public void YellowBanacoinRoutes_AreOwnedByYellowAdapter()
    {
        var routes = ProtocolRouteTestHelper.FindPostRoutes(typeof(TaikoLocalServer.Adapters.GameProtocol.Yellow.DependencyInjection).Assembly)
            .Where(route => route.Template.Contains("banacoin", StringComparison.OrdinalIgnoreCase)
                || route.Template.EndsWith("/balancecheck.php", StringComparison.OrdinalIgnoreCase))
            .Select(route => route.Template)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            [
                "/v09r02/chassis/balancecheck.php",
                "/v09r02/chassis/banacoinerrorlog.php",
                "/v09r02/chassis/banacoinpayment.php",
                "/v09r02/chassis/getbanacoininfo.php"
            ],
            routes);
    }

    private static TController CreateController<TController>() where TController : ControllerBase, new()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();

        return new TController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = services }
            }
        };
    }
}
