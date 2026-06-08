using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Adapters.GameProtocol.Yellow.Controllers;
using TaikoLocalServer.Adapters.GameProtocol.Yellow.Wire;
using TaikoLocalServer.Tests.Blue;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowBanacoinCompatibilityTests
{
    [Theory]
    [InlineData("BalanceCheckController", "Yellow BalanceCheck request: {@Request}")]
    [InlineData("BanacoinPaymentController", "Yellow BanacoinPayment request: {@Request}")]
    [InlineData("BanacoinErrorLogController", "Yellow BanacoinErrorLog request: {@Request}")]
    [InlineData("GetBanacoinInfoController", "Yellow GetBanacoinInfo request: {@Request}")]
    public void YellowBanacoinControllers_LogFullRequestObjects(string controllerName, string expectedLog)
    {
        var controller = ExtractControllerSource(controllerName);

        Assert.Contains(expectedLog, controller, StringComparison.Ordinal);
        Assert.DoesNotContain("request from {ChassisId}", controller, StringComparison.Ordinal);
    }

    [Fact]
    public void YellowBanacoinControllers_ReturnMinimalStatelessSuccessResponses()
    {
        var source = File.ReadAllText(ControllerPath());

        Assert.Contains(
            "new BalancecheckResponse { Result = 1, Personid = request.Personid }",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "new BanacoinpaymentResponse { Result = 1, Personid = request.Personid }",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "new BanacoinerrorlogResponse { Result = 1 }",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "new GetbanacoininfoResponse { Result = 1 }",
            source,
            StringComparison.Ordinal);

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
                "/v09r00/chassis/balancecheck.php",
                "/v09r00/chassis/banacoinerrorlog.php",
                "/v09r00/chassis/banacoinpayment.php",
                "/v09r00/chassis/getbanacoininfo.php"
            ],
            routes);
    }

    [Theory]
    [InlineData("BalanceCheckController")]
    [InlineData("BanacoinPaymentController")]
    [InlineData("BanacoinErrorLogController")]
    [InlineData("GetBanacoinInfoController")]
    public void YellowBanacoinControllers_DoNotCallMediatorOrPersistence(string controllerName)
    {
        var controller = ExtractControllerSource(controllerName);

        Assert.DoesNotContain("Mediator.Send", controller, StringComparison.Ordinal);
        Assert.DoesNotContain("ITaikoDbContext", controller, StringComparison.Ordinal);
        Assert.DoesNotContain("DbContext", controller, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveChanges", controller, StringComparison.Ordinal);
        Assert.DoesNotContain("Wallet", controller, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Settlement", controller, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Receipt", controller, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Transaction", controller, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Bnid", controller, StringComparison.Ordinal);
        Assert.DoesNotContain("Chid", controller, StringComparison.Ordinal);
    }

    [Fact]
    public void YellowBanacoinControllers_DoNotPopulateOptionalPaymentOrIdentityFields()
    {
        foreach (var controllerName in new[]
        {
            "BalanceCheckController",
            "BanacoinPaymentController",
            "BanacoinErrorLogController",
            "GetBanacoinInfoController"
        })
        {
            var controller = ExtractControllerSource(controllerName);

            Assert.DoesNotContain("BnidResult =", controller, StringComparison.Ordinal);
            Assert.DoesNotContain("Chid =", controller, StringComparison.Ordinal);
            Assert.DoesNotContain("CoinCoupon =", controller, StringComparison.Ordinal);
            Assert.DoesNotContain("PlayerType =", controller, StringComparison.Ordinal);
            Assert.DoesNotContain("ComSvrResult =", controller, StringComparison.Ordinal);
            Assert.DoesNotContain("MbId =", controller, StringComparison.Ordinal);
            Assert.DoesNotContain("Baid =", controller, StringComparison.Ordinal);
            Assert.DoesNotContain("AccessCode =", controller, StringComparison.Ordinal);
            Assert.DoesNotContain("IsPublish =", controller, StringComparison.Ordinal);
            Assert.DoesNotContain("CardOwnNum =", controller, StringComparison.Ordinal);
            Assert.DoesNotContain("RegCountryId =", controller, StringComparison.Ordinal);
            Assert.DoesNotContain("PurposeId =", controller, StringComparison.Ordinal);
            Assert.DoesNotContain("RegionId =", controller, StringComparison.Ordinal);
        }
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

    private static string ExtractControllerSource(string controllerName)
    {
        var source = File.ReadAllText(ControllerPath());
        var start = source.IndexOf($"class {controllerName}", StringComparison.Ordinal);
        Assert.True(start >= 0, $"Controller {controllerName} not found.");
        var next = source.IndexOf("\n[ApiController]", start, StringComparison.Ordinal);
        return next >= 0 ? source[start..next] : source[start..];
    }

    private static string ControllerPath()
    {
        return Path.Combine(
            FindRepoRoot(),
            "Adapters.GameProtocol.Yellow",
            "Controllers",
            "YellowScaffoldControllers.cs");
    }

    private static string FindRepoRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "TaikoLocalServer.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("Could not find TaikoLocalServer.slnx.");
    }
}
