namespace TaikoLocalServer.Application.Ac15;

public static class Ac15MapperNormalization
{
    public static List<uint> UIntList(uint[]? values) => values?.ToList() ?? [];

    public static uint UIntOrZero(uint? value) => value.GetValueOrDefault();

    public static uint UInt(uint value) => value;

    public static uint[] ToUIntArray(List<uint> value) => value.ToArray();

    public static Difficulty FromProtocolDifficulty(uint value) => Ac15Difficulty.FromProtocol(value);

    public static uint ToProtocolDifficulty(Difficulty value) => Ac15Difficulty.ToProtocol(value);

    public static byte[] BytesOrEmpty(byte[]? values) => values ?? [];

    public static string StringOrEmpty(string? value) => value ?? string.Empty;

    public static string PresentString(string? value) => string.IsNullOrEmpty(value) ? null! : value;

    public static uint? PositivePlayDan(uint value) => value > 0 ? value : null;

    public static uint? PositiveNullablePlayDan(uint? value) => value is > 0 ? value : null;

    public static uint DisplayDan(uint value, Ac15ProtocolLimits limits)
        => value >= limits.MinNormalDanId && value <= limits.MaxNormalDanId
            ? value
            : limits.SafeDisplayDanFallback;

    public static byte[] FixedOrZero(byte[]? value, int byteCount)
        => Ac15ProtocolBytes.FixedOrZero(value, byteCount);

    public static List<uint> DecodeBitset(byte[]? values, int byteCount)
        => BitsetCodec.Decode(values, byteCount);
}
