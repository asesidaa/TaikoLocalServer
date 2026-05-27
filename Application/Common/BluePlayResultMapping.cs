namespace TaikoLocalServer.Application.Common;

public static class BluePlayResultMapping
{
    public static Difficulty MapDifficulty(uint level) => level switch
    {
        1 => Difficulty.Easy,
        2 => Difficulty.Normal,
        3 => Difficulty.Hard,
        4 => Difficulty.Oni,
        5 => Difficulty.UraOni,
        _ => Difficulty.None
    };

    public static CrownType MapCrown(uint playResult) => playResult switch
    {
        1 => CrownType.Clear,
        2 => CrownType.Gold,
        3 => CrownType.Dondaful,
        _ => CrownType.None
    };

    public static bool IsSupportedNormalStageMode(uint stageMode) => stageMode is 0 or 1;

    public static bool IsShin(uint stageMode) => stageMode == 1;

    public static int CrownRank(CrownType crown) => crown switch
    {
        CrownType.Clear => 1,
        CrownType.Gold => 2,
        CrownType.Dondaful => 3,
        _ => 0
    };

    public static BlueCrownState MapBlueCrownState(CrownType crown) => crown switch
    {
        CrownType.Clear => BlueCrownState.Clear,
        CrownType.Gold => BlueCrownState.FullCombo,
        CrownType.Dondaful => BlueCrownState.FullCombo,
        _ => BlueCrownState.None
    };
}
