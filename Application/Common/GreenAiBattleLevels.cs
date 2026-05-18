namespace TaikoLocalServer.Application.Common;

public static class GreenAiBattleLevels
{
    public static bool IsCertifiedLevel(uint sdCertifiedLevelId)
        => sdCertifiedLevelId is 1 or 5 or 9 or 13;
}
