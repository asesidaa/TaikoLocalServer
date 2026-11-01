using System.Security.Cryptography;
using System.Text;
using OtpNet;

namespace TaikoLocalServer.Adapters.AdminApi.Authorization;

/// <summary>Authenticator (TOTP, 30 s, 6 digits) and recovery codes for 2-step verification.</summary>
public static class TwoFactorCodes
{
    private const int RecoveryCodeCount = 10;
    private const string Alphabet = "23456789ABCDEFGHJKMNPQRSTUVWXYZ";

    public static string NewSecret() => Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));

    public static string Uri(string secret, string accountName) =>
        $"otpauth://totp/{System.Uri.EscapeDataString("TaikoLocalServer:" + accountName)}?secret={secret}&issuer=TaikoLocalServer";

    /// <summary>A code from the app for <paramref name="secret"/>, one step either side for clock drift; the step it matched, or null.</summary>
    public static long? MatchTotp(string secret, string code)
    {
        try
        {
            return new Totp(Base32Encoding.ToBytes(secret)).VerifyTotp(Digits(code), out var step, new VerificationWindow(1, 1)) ? step : null;
        }
        catch (ArgumentException)
        {
            return null; // not base32
        }
    }

    /// <summary>
    /// A 6-digit code (not one already used) or an unused recovery code. Updates the credential (last step,
    /// spent recovery code); the caller saves it.
    /// </summary>
    public static bool Verify(Credential credential, string code)
    {
        if (credential.TotpSecret is not { } secret) return false;
        if (code.All(c => char.IsAsciiDigit(c) || c == ' '))
        {
            if (MatchTotp(secret, code) is not { } step || step <= credential.TotpLastStep) return false;
            credential.TotpLastStep = step;
            return true;
        }
        var left = credential.RecoveryCodes.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
        if (!left.Remove(Hash(code))) return false;
        credential.RecoveryCodes = string.Join('\n', left);
        return true;
    }

    public static int RecoveryCodesLeft(Credential credential) =>
        credential.RecoveryCodes.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length;

    /// <summary>Replaces the recovery codes; returns them (XXXXX-XXXX) to show once.</summary>
    public static List<string> NewRecoveryCodes(Credential credential)
    {
        var codes = Enumerable.Range(0, RecoveryCodeCount).Select(_ =>
        {
            var c = new string(Enumerable.Range(0, 9).Select(_ => Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)]).ToArray());
            return $"{c[..5]}-{c[5..]}";
        }).ToList();
        credential.RecoveryCodes = string.Join('\n', codes.Select(Hash));
        return codes;
    }

    public static void Clear(Credential credential)
    {
        credential.TotpSecret = null;
        credential.TotpLastStep = 0;
        credential.RecoveryCodes = string.Empty;
    }

    private static string Digits(string code) => new(code.Where(char.IsAsciiDigit).ToArray());

    // Recovery codes are random (about 44 bits), so a plain hash is enough; dashes and case are ignored.
    private static string Hash(string code) =>
        Convert.ToHexString(SHA256.HashData(Encoding.ASCII.GetBytes(new string(code.Where(char.IsAsciiLetterOrDigit).ToArray()).ToUpperInvariant())));
}
