using QRCoder;

namespace TaikoLocalServer.Adapters.AdminApi.Authorization;

public static class QrImage
{
    /// <summary><paramref name="text"/> as a PNG QR code, a data: URI for an img src.</summary>
    public static string DataUri(string text, QRCodeGenerator.ECCLevel level = QRCodeGenerator.ECCLevel.M, int pixelsPerModule = 6)
    {
        using var data = new QRCodeGenerator().CreateQrCode(text, level);
        return "data:image/png;base64," + Convert.ToBase64String(new PngByteQRCode(data).GetGraphic(pixelsPerModule));
    }
}
