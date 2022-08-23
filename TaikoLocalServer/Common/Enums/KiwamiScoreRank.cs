namespace TaikoLocalServer.Common.Enums;

[Flags]
public enum KiwamiScoreRank : byte
{
    None   = 0,
    Easy   = 1,
    Normal = 2,
    Hard   = 4,
    Oni    = 8,
    UraOni = 16
}