namespace TaikoLocalServer.Common.Enums;

[Flags]
public enum MiyabiScoreRank : ushort
{
    None           = 0,
    EasyGold       = 0b01,
    EasySakura     = 0b10,
    EasyPurple     = 0b11,
    NormalGold     = 0b01 << 2,
    NormalSakura   = 0b10 << 2,
    NormalPurple   = 0b11 << 2,
    HardGold       = 0b01 << 4,
    HardSakura     = 0b10 << 4,
    HardPurple     = 0b11 << 4,
    OniGold        = 0b01 << 6,
    OniSakura      = 0b10 << 6,
    OniPurple      = 0b11 << 6,
    UraOniGold     = 0b01 << 8,
    UraOniSakura   = 0b10 << 8,
    UraOniPurple   = 0b11 << 8
}