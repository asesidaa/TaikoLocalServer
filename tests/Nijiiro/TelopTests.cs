using TaikoLocalServer.Application.ServerData;

namespace TaikoLocalServer.Tests.Nijiiro;

// The 39.06 cabinet's ticker: it shows only text tagged "/key " (keys S, Cp, C, Mp, M; any other key crashes the game,
// seen with "/J" on opening the test menu), so the announcement live now is served once per key, always tagged. Its
// version must change when the live message changes, or cabinets keep the old one.
public sealed class TelopTests
{
    private static TelopData Telop(uint id, string start, string end, string text, uint verup = 1) =>
        new() { TelopId = id, VerupNo = verup, StartDatetime = start, EndDatetime = end, Telop = text };

    private static readonly DateTime Now = new(2026, 11, 3, 12, 0, 0);

    [Fact]
    public void TheLiveMessage_SkipsEmptyEndedAndScheduledOnes()
    {
        var telops = new[]
        {
            Telop(1, "20000101000000", "20991231235959", " "), // the shipped placeholder
            Telop(2, "20261101000000", "20261102000000", "Ended"),
            Telop(3, "20261103000000", "20261110000000", "Season 2 starts Friday"),
            Telop(4, "20261105000000", "20261110000000", "Scheduled"),
        };

        Assert.Equal("Season 2 starts Friday", NijiiroTelop.Live(telops, Now)?.Telop);
        Assert.Null(NijiiroTelop.Live(telops.Take(2), Now));
        Assert.Equal(0u, NijiiroTelop.Version(null));
    }

    [Fact]
    public void EachTelop_IsTheMessageTaggedWithASafeKey_EvenWhenTheTextStartsWithASlash()
    {
        Assert.Equal(["/S Hello", "/Cp Hello", "/C Hello", "/Mp Hello", "/M Hello"], NijiiroTelop.Ids.Select(id => NijiiroTelop.Text(id, "Hello")));
        // A typed tag stays text after the safe one (an unknown key would crash the cabinet).
        Assert.Equal("/S /J Hello", NijiiroTelop.Text(1, "/J Hello"));
        // One line: the cabinet reads to the end of the line.
        Assert.Equal("/S A B", NijiiroTelop.Text(1, "A\r\n B\n"));
        Assert.Null(NijiiroTelop.Text(6, "Hello"));
    }

    [Fact]
    public void TheVersion_ChangesWhenTheLiveMessageIsEditedOrAnotherGoesLive()
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
