namespace TaikoLocalServer.Application.Common;

public static class GreenPlayResultMapping
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

    public static GreenCrownState MapGreenCrownState(CrownType crown) => crown switch
    {
        CrownType.Clear => GreenCrownState.Clear,
        CrownType.Gold => GreenCrownState.FullCombo,
        // Green presents all-good like full combo; state 3 is not proven safe by the binary notes.
        CrownType.Dondaful => GreenCrownState.FullCombo,
        _ => GreenCrownState.None
    };
}
