namespace TaikoLocalServer.Adapters.GameProtocol.Shared.Compression;

public static class HeaderStripUtil
{
    /// <summary>
    /// Strips the 32-byte header used by the WW R08 protocol's request envelopes.
    /// </summary>
    public static byte[] StripWwHeader(byte[] payload) => payload.Skip(32).ToArray();
}
