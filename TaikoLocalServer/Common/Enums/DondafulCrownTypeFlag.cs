namespace TaikoLocalServer.Common.Enums;

[Flags]
public enum DondafulCrownTypeFlag : byte
{
    None     = 0,
    Easy     = 1,
    Normal   = 1 << 1,
    Hard     = 1 << 2,
    Oni      = 1 << 3,
    UraOni   = 1 << 4
}