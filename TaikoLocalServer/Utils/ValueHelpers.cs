namespace TaikoLocalServer.Utils;

public static class ValueHelpers
{
    public static uint GetNonZeroValue(uint val)
    {
        return val == 0 ? 1 : val;
    }
}