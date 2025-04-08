using System.Security.Cryptography;
using ZXingCpp;

namespace TaikoLocalServer.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class QRCodeController : BaseController<QRCodeController>
{
    
    [HttpGet("id/{id}")]
    public IActionResult GenerateIdQRCode(string id)
    {
        var barcode = new Barcode("BNTTCNID" + id, BarcodeFormats.QRCode);
        return Content(barcode.ToSVG(), "image/svg+xml");
    }

    [HttpGet("serial/{serial}")]
    public IActionResult GenerateSerialQRCode(string serial)
    {
        byte[] byteArray = System.Text.Encoding.ASCII.GetBytes(serial);
        byte[] data = new byte[7 + byteArray.Length];
        writeByteArray(data, 0, [ 
            0xFF, 0xFF, 
            (byte)(serial.Length & 0xFF), 
            0x01, 0x00 
        ]);
        writeByteArray(data, 5, byteArray);
        writeByteArray(data, 5 + byteArray.Length, [0xEE, 0xFF]);

        data = encode(data, key, iv);
        byte[] finalData = new byte[5 + data.Length];
        writeString(finalData, 0, "S12");
        writeByteArray(finalData, 3, [0x00, 0x01]);
        writeByteArray(finalData, 5, data);

        var barcode = new Barcode(finalData, BarcodeFormats.QRCode);
        return Content(barcode.ToSVG(), "image/svg+xml");
    }

    private byte[] key = { 0xB2, 0x4F, 0x9B, 0x16, 0xFF, 0xB5, 0xB1, 0x32, 0x2C, 0x4B, 0x06, 0x80, 0x8B, 0xE1, 0xF2, 0x6A };
    private byte[] iv = { 0x7F, 0xC8, 0xD8, 0xE2, 0x00, 0x1E, 0x1C, 0x88, 0xEE, 0x5C, 0xD9, 0x48, 0xE2, 0x9F, 0x65, 0xFD };
    private void writeString(byte[] data, int begin, string toWrite)
    {
        byte[] byteArray = System.Text.Encoding.ASCII.GetBytes(toWrite);
        writeByteArray(data, begin, byteArray);
    }

    private void writeByteArray(byte[] data, int begin, byte[] byteArray)
    {
        for (int i = 0; i < byteArray.Length; i++)
        {
            data[begin + i] = byteArray[i];
        }
    }

    private void writeBytes(byte[] data, int begin, byte b, int size)
    {
        for (int i = 0; i < size; i++)
        {
            data[begin + i] = b;
        }
    }

    private byte[] pad(byte[] data, uint align)
    {
        long paddedSize = ((data.Length / align) * align) + ((data.Length % align) > 0 ? align : 0);
        if (paddedSize == data.Length) return data;

        byte[] padded = new byte[paddedSize];
        writeByteArray(padded, 0, data);
        writeBytes(padded, data.Length, 13, (int)(paddedSize - data.Length));

        return padded;
    }

    private byte[] encode(byte[] data, byte[] key, byte[] iv)
    {
        byte[] padded = pad(data, 16);
        byte[] encrypted;

        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    csEncrypt.Write(padded, 0, padded.Length);
                }
                encrypted = msEncrypt.ToArray();
            }
        }

        return encrypted;
    }
}