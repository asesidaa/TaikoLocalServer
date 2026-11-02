using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Adapters.AllnetMucha.Controllers.AmUpdater;
using TaikoLocalServer.Adapters.AllnetMucha.Wire;
using TaikoLocalServer.Infrastructure.Settings;

namespace TaikoLocalServer.Tests.AllnetMucha;

public sealed class MuchaControllerTests
{
    [Fact]
    public void DownloadState_ReturnsMethodNotAllowedInsteadOfAcknowledgingStaleChunkState()
    {
        var controller = CreateController();

        var result = controller.DownloadState();

        var status = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status405MethodNotAllowed, status.StatusCode);
    }

    [Fact]
    public void RegiAuth_ReturnsInternationalMuchaSuccessWithEncryptedNonZeroTokens()
    {
        var controller = CreateController();

        var result = controller.RegiAuth(new MuchaRegiAuthRequest
        {
            SendDate = "20260607"
        });

        var fields = ParseFormOutput(result);
        Assert.Equal("001", fields["RESULTS"]);
        Assert.Equal("4C49D33559D5F7AF", fields["ALL_TOKEN"]);
        Assert.Equal("4C49D33559D5F7AF", fields["ADD_TOKEN"]);
        Assert.NotEqual("0", fields["ALL_TOKEN"]);
        Assert.NotEqual("0", fields["ADD_TOKEN"]);
        Assert.NotEqual("999", fields["ALL_TOKEN"]);
        Assert.NotEqual("999", fields["ADD_TOKEN"]);
    }

    [Fact]
    public void TokenState_ReturnsInternationalMuchaSuccess()
    {
        var controller = CreateController();

        var result = controller.TokenState();

        var fields = ParseFormOutput(result);
        Assert.Equal("001", fields["RESULTS"]);
    }

    [Fact]
    public void TokenMarginState_ReturnsInternationalMuchaSuccessWithZeroMargins()
    {
        var controller = CreateController();

        var result = controller.TokenMarginState();

        var fields = ParseFormOutput(result);
        Assert.Equal("001", fields["RESULTS"]);
        Assert.Equal("0", fields["LIMIT_LOWER_TOKEN"]);
        Assert.Equal("0", fields["LIMIT_UPPER_TOKEN"]);
        Assert.Equal("0", fields["LAST_SETTLEMENT_MONTH"]);
        Assert.Equal("0", fields["LAST_LIMIT_LOWER_TOKEN"]);
        Assert.Equal("0", fields["LAST_LIMIT_UPPER_TOKEN"]);
        Assert.Equal("0", fields["SETTLEMENT_MONTH"]);
    }

    private static Dictionary<string, string> ParseFormOutput(ContentResult result)
    {
        Assert.NotNull(result.Content);
        return result.Content.Split('&')
            .Select(pair => pair.Split('=', 2))
            .ToDictionary(pair => pair[0], pair => pair.Length == 2 ? pair[1] : string.Empty);
    }

    private static MuchaController CreateController()
    {
        return new MuchaController(Options.Create(new AllnetSettings()))
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    RequestServices = new ServiceCollection()
                        .AddLogging()
                        .BuildServiceProvider()
                }
            }
        };
    }
}
