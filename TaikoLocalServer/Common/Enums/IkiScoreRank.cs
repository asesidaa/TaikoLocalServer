namespace TaikoLocalServer.Common.Enums;

public enum IkiScoreRank : ushort
{
    None            = 0,
    EasyWhite       = 0b01,
    EasyBronze      = 0b10,
    EasySilver      = 0b11,
    NormalWhite     = 0b01 << 2,
    NormalBronze    = 0b10 << 2,
    NormalSilver    = 0b11 << 2,
    HardWhite       = 0b01 << 4,
    HardBronze      = 0b10 << 4,
    HardSilver      = 0b11 << 4,
    OniWhite        = 0b01 << 6,
    OniBronze       = 0b10 << 6,
    OniSilver       = 0b11 << 6,
    UraOniWhite     = 0b01 << 8,
    UraOniBronze    = 0b10 << 8,
    UraOniSilver    = 0b11 << 8
}