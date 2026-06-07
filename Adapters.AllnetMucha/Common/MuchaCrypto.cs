using System.Diagnostics.CodeAnalysis;
using System.Text;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace TaikoLocalServer.Adapters.AllnetMucha.Common;

internal static class MuchaCrypto
{
    private const int BlowfishBlockSize = 8;

    public static string EncryptTokenValue(string tokenValue, string sendDate)
    {
        var key = DeriveSendDateKey(sendDate);
        var plaintext = PadToBlockSize(Encoding.ASCII.GetBytes(tokenValue));
        var ciphertext = new byte[plaintext.Length];

        var cipher = new CbcBlockCipher(new BlowfishEngine());
        cipher.Init(true, new ParametersWithIV(new KeyParameter(key), key));

        for (var offset = 0; offset < plaintext.Length; offset += BlowfishBlockSize)
        {
            cipher.ProcessBlock(plaintext, offset, ciphertext, offset);
        }

        return Convert.ToHexString(ciphertext);
    }

    public static bool HasUsableSendDate([NotNullWhen(true)] string? sendDate) => sendDate is { Length: >= BlowfishBlockSize };

    private static byte[] DeriveSendDateKey(string sendDate)
    {
        var key = new byte[BlowfishBlockSize];
        for (var i = 0; i < key.Length; i++)
        {
            key[i] = (byte)sendDate[(i - 1) & 7];
        }

        return key;
    }

    private static byte[] PadToBlockSize(byte[] plaintext)
    {
        var paddedLength = Math.Max(BlowfishBlockSize, ((plaintext.Length + BlowfishBlockSize - 1) / BlowfishBlockSize) * BlowfishBlockSize);
        var padded = new byte[paddedLength];
        plaintext.CopyTo(padded, 0);
        return padded;
    }
}
