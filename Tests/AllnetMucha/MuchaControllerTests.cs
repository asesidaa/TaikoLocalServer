using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Adapters.AllnetMucha.Controllers.AmUpdater;
using TaikoLocalServer.Adapters.AllnetMucha.Wire;
using TaikoLocalServer.Infrastructure.Settings;
using System.Reflection;

namespace TaikoLocalServer.Tests.AllnetMucha;

public sealed class MuchaControllerTests
{
    [Fact]
    public void DownloadState_ReturnsMethodNotAllowedInsteadOfAcknowledgingStaleChunkState()
    {
        var controller = new MuchaController(Options.Create(new AllnetSettings()));

        var result = controller.DownloadState();

        var status = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status405MethodNotAllowed, status.StatusCode);
    }

    [Fact]
    public void RegiAuth_ReturnsInternationalMuchaSuccessWithEncryptedNonZeroTokens()
    {
        var controller = new MuchaController(Options.Create(new AllnetSettings()));

        var result = controller.RegiAuth(new MuchaRegiAuthRequest
        {
            SendDate = "20260607"
        });

        var fields = ParseFormOutput(result);
        Assert.Equal("001", fields["RESULTS"]);
        Assert.Equal("54AAA330BABF5AAF", fields["ALL_TOKEN"]);
        Assert.Equal("54AAA330BABF5AAF", fields["ADD_TOKEN"]);
        Assert.NotEqual("0", fields["ALL_TOKEN"]);
        Assert.NotEqual("0", fields["ADD_TOKEN"]);
        Assert.NotEqual("999", fields["ALL_TOKEN"]);
        Assert.NotEqual("999", fields["ADD_TOKEN"]);
    }

    [Fact]
    public void TokenState_ReturnsInternationalMuchaSuccess()
    {
        var controller = new MuchaController(Options.Create(new AllnetSettings()));

        var result = controller.TokenState();

        var fields = ParseFormOutput(result);
        Assert.Equal("001", fields["RESULTS"]);
    }

    [Fact]
    public void TokenMarginState_ReturnsInternationalMuchaSuccessWithZeroMargins()
    {
        var controller = new MuchaController(Options.Create(new AllnetSettings()));

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

    [Theory]
    [InlineData(nameof(MuchaController.RegiAuth), "/mucha_front/regiauth.do")]
    [InlineData(nameof(MuchaController.TokenState), "/mucha_front/tokenstate.do")]
    [InlineData(nameof(MuchaController.TokenMarginState), "/mucha_front/tokenmarginstate.do")]
    public void InternationalMuchaTokenEndpoints_UseExpectedRoutes(string actionName, string expectedRoute)
    {
        var method = typeof(MuchaController).GetMethod(actionName, BindingFlags.Instance | BindingFlags.Public);
        Assert.NotNull(method);

        var attribute = Assert.Single(method.GetCustomAttributes<HttpPostAttribute>());
        Assert.Equal(expectedRoute, attribute.Template);
    }

    private static Dictionary<string, string> ParseFormOutput(ContentResult result)
    {
        Assert.NotNull(result.Content);
        return result.Content.Split('&')
            .Select(pair => pair.Split('=', 2))
            .ToDictionary(pair => pair[0], pair => pair.Length == 2 ? pair[1] : string.Empty);
    }
}
