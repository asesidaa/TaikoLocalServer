namespace TaikoLocalServer.Common.Enums;

[Flags]
public enum CrownTypeFlag : ushort
{
    None         = 0,
    EasyClear    = 0b10,
    EasyGold     = 0b11,
    NormalClear  = 0b10_00,
    NormalGold   = 0b11_00,
    HardClear    = 0b10_0000,
    HardGold     = 0b11_0000,
    OniClear     = 0b10_00_0000,
    OniGold      = 0b11_00_0000,
    UraOniClear  = 0b10_00000000,
    UraOniGold   = 0b11_00000000
}