using System.Text;
using ICSharpCode.SharpZipLib.GZip;

namespace TaikoLocalServer.Utils;

public static class GZipBytesUtil
{
    public static byte[] GetEmptyJsonGZipBytes()
    {
        var outputStream = new MemoryStream(1024);
        using (var stream = new GZipOutputStream(outputStream))
        using (var writer = new StreamWriter(stream, Encoding.UTF8))
        {
            /*writer.AutoFlush = true;
            writer.WriteLine("{}");*/
            stream.Write(Array.Empty<byte>());
        }

        return outputStream.ToArray();
    }

    public static byte[] GetGZipBytes(ReadOnlySpan<byte> input)
    {
        var outputStream = new MemoryStream(1024);
        using (var stream = new GZipOutputStream(outputStream))
        {
            stream.Write(input);
        }

        return outputStream.ToArray();
    }
}