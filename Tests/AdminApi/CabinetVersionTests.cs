using Microsoft.AspNetCore.Http;
using TaikoLocalServer.Adapters.GameProtocol.Shared.Cabinets;

namespace TaikoLocalServer.Tests.AdminApi;

public sealed class CabinetVersionTests
{
    // The admin UI reads a cabinet's game from the first word, so each game's routes must start with its name.
    [Theory]
    [InlineData("/v12r08_ww/chassis/heartbeat", "WW 39.06")]
    [InlineData("/v12r00_cn/chassis/heartbeat", "CHN")]
    [InlineData("/v11r01/chassis/userdata.php", "Green 11.01")]
    [InlineData("/v08r00_tw/chassis/heartbeat.php", "Red 08.00 TW")]
    [InlineData("/v07r03/chassis/heartbeat.php", "White 07.03")]
    [InlineData("/v04r00/chassis/heartbeat.php", "Momoiro 04.00")]
    [InlineData("/v01r00/chassis/startupauth.php", "")]
    public void VersionOf_NamesTheGame(string path, string version) =>
        Assert.Equal(version, CabinetFilter.VersionOf(new PathString(path)));
}
