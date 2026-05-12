using System.IO.Compression;

namespace TaikoLocalServer.Adapters.GameProtocol.Green;

public static class GreenPlayResultPayloadDecoder
{
    public static GreenPlayResultPayloadDecodeResult Decode(byte[] payload)
    {
        try
        {
            var bytes = InflateGzip(payload ?? []);
            var request = Serializer.Deserialize<PlayResultDataRequest>(new ReadOnlySpan<byte>(bytes));
            return new GreenPlayResultPayloadDecodeResult(request, "gzip", bytes.Length);
        }
        catch (Exception ex)
        {
            throw new GreenPlayResultPayloadDecodeException(ex);
        }
    }

    public static string HexPreview(byte[] payload, int maxBytes = 32)
    {
        if (payload.Length == 0)
        {
            return "";
        }

        var preview = payload.AsSpan(0, Math.Min(payload.Length, maxBytes));
        return Convert.ToHexString(preview);
    }

    public static string BuildReceivedDump(PlayResultDataRequest request, CommonPlayResultData common)
    {
        return string.Join(
            Environment.NewLine,
            "Green PlayResultDataRequest:",
            request.Stringify(),
            "CommonPlayResultData:",
            common.Stringify());
    }

    private static byte[] InflateGzip(byte[] payload)
    {
        using var input = new MemoryStream(payload);
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        gzip.CopyTo(output);
        return output.ToArray();
    }
}

public sealed record GreenPlayResultPayloadDecodeResult(
    PlayResultDataRequest Request,
    string Format,
    int DecodedBytes);

public sealed class GreenPlayResultPayloadDecodeException(Exception innerException)
    : InvalidOperationException("Unable to decode gzip-compressed Green playresult_data payload.", innerException)
{
}
