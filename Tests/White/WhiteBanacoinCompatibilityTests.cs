using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Adapters.GameProtocol.White.Controllers;
using TaikoLocalServer.Adapters.GameProtocol.White.Wire;

namespace TaikoLocalServer.Tests.White;

public sealed class WhiteBanacoinCompatibilityTests
{
    [Fact]
    public void BalanceCheck_ReturnsStatelessSuccessShape()
    {
        var response = Invoke(
            new BalanceCheckController(),
            controller => controller.BalanceCheck(new BalancecheckRequest { Personid = "person" }),
            value => Assert.IsType<BalancecheckResponse>(value));

        Assert.Equal(1u, response.Result);
        Assert.Equal("person", response.Personid);
        Assert.Equal("Ok", response.BnidResult);
        Assert.Equal(9999u, response.CoinCoupon);
    }

    [Fact]
    public void BanacoinPayment_ReturnsStatelessSuccessShape()
    {
        var response = Invoke(
            new BanacoinPaymentController(),
            controller => controller.BanacoinPayment(new BanacoinpaymentRequest { Personid = "person" }),
            value => Assert.IsType<BanacoinpaymentResponse>(value));

        Assert.Equal(1u, response.Result);
        Assert.Equal("person", response.Personid);
        Assert.Equal("Ok", response.BnidResult);
        Assert.Equal("1", response.Chid);
    }

    [Fact]
    public void BanacoinErrorLog_ReturnsStatelessSuccessShape()
    {
        var response = Invoke(
            new BanacoinErrorLogController(),
            controller => controller.BanacoinErrorLog(new BanacoinerrorlogRequest()),
            value => Assert.IsType<BanacoinerrorlogResponse>(value));

        Assert.Equal(1u, response.Result);
    }

    [Fact]
    public void GetBanacoinInfo_ReturnsResultOnlyShape()
    {
        var response = Invoke(
            new GetBanacoinInfoController(),
            controller => controller.GetBanacoinInfo(new GetbanacoininfoRequest()),
            value => Assert.IsType<GetbanacoininfoResponse>(value));

        Assert.Equal(1u, response.Result);
        Assert.False(response.ShouldSerializePlayerType());
        Assert.False(response.ShouldSerializeComSvrResult());
        Assert.False(response.ShouldSerializeMbId());
        Assert.False(response.ShouldSerializeBaid());
        Assert.False(response.ShouldSerializeAccessCode());
        Assert.False(response.ShouldSerializeIsPublish());
        Assert.False(response.ShouldSerializeCardOwnNum());
        Assert.False(response.ShouldSerializeRegCountryId());
        Assert.False(response.ShouldSerializePurposeId());
        Assert.False(response.ShouldSerializeRegionId());
        Assert.False(response.ShouldSerializePersonid());
    }

    private static TResponse Invoke<TController, TResponse>(
        TController controller,
        Func<TController, IActionResult> action,
        Func<object?, TResponse> assertResponse)
        where TController : ControllerBase
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                RequestServices = new ServiceCollection()
                    .AddLogging()
                    .BuildServiceProvider()
            }
        };

        var ok = Assert.IsType<OkObjectResult>(action(controller));
        return assertResponse(ok.Value);
    }
}
