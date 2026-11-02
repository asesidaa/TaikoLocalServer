namespace TaikoLocalServer.Application.Common;

public static class GreenAiBattleLevels
{
    public static bool AllowsCrown(Difficulty courseLevel, uint supportLevel)
        => courseLevel == Difficulty.UraOni || supportLevel == 0;
}
