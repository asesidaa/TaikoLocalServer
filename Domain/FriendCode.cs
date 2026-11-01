using System.Security.Cryptography;

namespace TaikoLocalServer.Domain;

/// <summary>
/// A user's shareable friend code: 12 random digits, unique, shown in groups of four. Random rather than derived
/// from the Baid so it can be regenerated, and never the access code (that's a login secret).
/// </summary>
public static class FriendCode
{
    public const int Length = 12;

    public static string New() => string.Concat(Enumerable.Range(0, Length).Select(_ => (char)('0' + RandomNumberGenerator.GetInt32(10))));
}
