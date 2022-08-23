namespace TaikoLocalServer.Common.Enums;

[Flags]
public enum CrownTypeFlag : ushort
{
    None         = 0,
    EasyClear    = 0b10,
    EasyGold     = 0b11,
    NormalClear  = 0b10 << 2,
    NormalGold   = 0b11 << 2,
    HardClear    = 0b10 << 4,
    HardGold     = 0b11 << 4,
    OniClear     = 0b10 << 6,
    OniGold      = 0b11 << 6,
    UraOniClear  = 0b10 << 8,
    UraOniGold   = 0b11 << 8
}