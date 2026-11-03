using TaikoLocalServer.Application.ServerData;

namespace TaikoLocalServer.Tests.Nijiiro;

// Nijiiro cabinets show one ticker message, telop 1 (a cabinet with nothing cached fetches only it), so telop 1 is the
// announcement live now. Its version must change when the live message changes, or cabinets keep the old one.
public sealed class TelopTests
{
    private static TelopData Telop(uint id, string start, string end, string text, uint verup = 1) =>
        new() { TelopId = id, VerupNo = verup, StartDatetime = start, EndDatetime = end, Telop = text };

    private static readonly DateTime Now = new(2026, 11, 3, 12, 0, 0);

    [Fact]
    public void Telop1_IsTheLiveMessage_SkippingEmptyEndedAndScheduledOnes()
    {
        var telops = new[]
        {
            Telop(1, "20000101000000", "20991231235959", ""), // the shipped placeholder
            Telop(2, "20261101000000", "20261102000000", "Ended"),
            Telop(3, "20261103000000", "20261110000000", "Season 2 starts Friday"),
            Telop(4, "20261105000000", "20261110000000", "Scheduled"),
        };

        var live = NijiiroTelop.Live(telops, Now);

        Assert.Equal("Season 2 starts Friday", live?.Telop);
        Assert.Null(NijiiroTelop.Live(telops.Take(2), Now));
        Assert.Equal(0u, NijiiroTelop.Version(null));
    }

    [Fact]
    public void Telop1sVersion_ChangesWhenTheLiveMessageIsEditedOrAnotherGoesLive()
    {
        var first = Telop(3, "20261103000000", "20261110000000", "A");
        var edited = Telop(3, "20261103000000", "20261110000000", "A, edited", verup: 2);
        var later = Telop(5, "20261103100000", "20261110000000", "B");

        Assert.NotEqual(NijiiroTelop.Version(first), NijiiroTelop.Version(edited));
        // The latest start wins when two are live.
        Assert.Equal(later, NijiiroTelop.Live([first, later], Now));
        Assert.NotEqual(NijiiroTelop.Version(first), NijiiroTelop.Version(later));
    }
}
