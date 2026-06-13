using System.Diagnostics.CodeAnalysis;
using System.Text;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;

namespace TaikoLocalServer.Adapters.AllnetMucha.Common;

internal static class MuchaCrypto
{
    private const int BlowfishBlockSize = 8;

    public static string EncryptTokenValue(string tokenValue, string sendDate)
    {
        var key = DeriveSendDateKey(sendDate);
        var plaintext = Encoding.ASCII.GetBytes(tokenValue);

        var cipher = new PaddedBufferedBlockCipher(new CbcBlockCipher(new BlowfishEngine()), new Pkcs7Padding());
        cipher.Init(true, new ParametersWithIV(new KeyParameter(key), key));

        var ciphertext = new byte[cipher.GetOutputSize(plaintext.Length)];
        var outputLength = cipher.ProcessBytes(plaintext, 0, plaintext.Length, ciphertext, 0);
        outputLength += cipher.DoFinal(ciphertext, outputLength);

        return Convert.ToHexString(ciphertext.AsSpan(0, outputLength));
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

}
