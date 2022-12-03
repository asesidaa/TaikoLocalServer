using System.Runtime.InteropServices;
using System.Text;
using ICSharpCode.SharpZipLib.GZip;

namespace TaikoLocalServer.Common.Utils;

public static class GZipBytesUtil
{
    public static MemoryStream GenerateStreamFromString(string value)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(value));
    }

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

    public static byte[] GetGZipBytes(byte[] input)
    {
        var outputStream = new MemoryStream(1024);
        using (var stream = new GZipOutputStream(outputStream))
        {
            stream.Write(input);
        }

        return outputStream.ToArray();
    }

    public static byte[] GetGZipBytes<T>(T[] data) where T : struct
    {
        var outputStream = new MemoryStream(1024);
        using (var stream = new GZipOutputStream(outputStream))
        {
            var byteRef = MemoryMarshal.AsBytes(new ReadOnlySpan<T>(data));
            stream.Write(byteRef);
        }

        return outputStream.ToArray();
    }

    public static byte[] DecompressGZipBytes(byte[] input)
    {
        using (var inputStream = new MemoryStream(input))
        using (var stream = new GZipInputStream(inputStream))
        using (var outputStream = new MemoryStream(1024))
        {
            stream.CopyTo(outputStream);
            return outputStream.ToArray();
        }
    }
}