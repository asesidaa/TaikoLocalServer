using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Adapters.AllnetMucha.Controllers.AmUpdater;
using TaikoLocalServer.Infrastructure.Settings;

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
}
