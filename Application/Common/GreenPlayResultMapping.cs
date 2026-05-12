namespace TaikoLocalServer.Application.Common;

public static class GreenPlayResultMapping
{
    public static Difficulty MapDifficulty(uint level) => level switch
    {
        0 => Difficulty.Easy,
        1 => Difficulty.Normal,
        2 => Difficulty.Hard,
        3 => Difficulty.Oni,
        4 => Difficulty.UraOni,
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
