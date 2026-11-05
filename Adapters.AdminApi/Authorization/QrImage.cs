using QRCoder;

namespace TaikoLocalServer.Adapters.AdminApi.Authorization;

public static class QrImage
{
    public static byte[] EventPayload(string serial)
    {
        if (string.IsNullOrEmpty(serial) || serial.Length > 255 || serial.Any(c => c < 0x20 || c > 0x7e))
            throw new ArgumentException("Event serial must be printable ASCII and at most 255 characters.", nameof(serial));
        var bytes = System.Text.Encoding.ASCII.GetBytes(serial);
        return [0x53, 0x31, 0x32, 0, 0, 0xff, 0xff, (byte)bytes.Length, 1, 0, .. bytes, 0xee, 0xff];
    }

    public static string DataUri(byte[] bytes, QRCodeGenerator.ECCLevel level = QRCodeGenerator.ECCLevel.M, int pixelsPerModule = 6)
    {
        using var data = new QRCodeGenerator().CreateQrCode(bytes, level);
        return "data:image/png;base64," + Convert.ToBase64String(new PngByteQRCode(data).GetGraphic(pixelsPerModule));
    }

    /// <summary><paramref name="text"/> as a PNG QR code, a data: URI for an img src.</summary>
    public static string DataUri(string text, QRCodeGenerator.ECCLevel level = QRCodeGenerator.ECCLevel.M, int pixelsPerModule = 6)
    {
        using var data = new QRCodeGenerator().CreateQrCode(text, level);
        return "data:image/png;base64," + Convert.ToBase64String(new PngByteQRCode(data).GetGraphic(pixelsPerModule));
    }
}
