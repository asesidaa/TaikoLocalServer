namespace TaikoLocalServer.Application.Common;

public static class GreenAiBattleLevels
{
    public static bool AllowsCrown(uint courseLevel, uint supportLevel)
        => courseLevel == 5 || supportLevel == 0;
}
