using TaikoLocalServer.Adapters.AdminApi.Authorization;

namespace TaikoLocalServer.Tests.AdminApi;

public sealed class EventQrTests
{
    [Fact]
    public void EventSerial_UsesBinaryS12Record()
    {
        Assert.Equal(new byte[] { 0x53, 0x31, 0x32, 0, 0, 0xff, 0xff, 4, 1, 0, 0x54, 0x45, 0x53, 0x54, 0xee, 0xff }, QrImage.EventPayload("TEST"));
        Assert.Throws<ArgumentException>(() => QrImage.EventPayload("bad\u00e9"));
        Assert.Throws<ArgumentException>(() => QrImage.EventPayload(new string('x', 256)));
    }
}
