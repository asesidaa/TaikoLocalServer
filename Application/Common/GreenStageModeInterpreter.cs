namespace TaikoLocalServer.Application.Common;

public static class GreenStageModeInterpreter
{
    public static bool IsShin(uint stageMode) => stageMode is 1 or 4;

    public static bool IsAiBattle(uint stageMode) => stageMode is 3 or 4;
}
