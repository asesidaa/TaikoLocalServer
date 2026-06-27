namespace TaikoLocalServer.Application.Common;

public static class GreenPlayResultMapping
{
    public static Difficulty MapDifficulty(uint level) => Ac15Difficulty.FromProtocol(level);

    public static CrownType MapCrown(uint playResult) => playResult switch
    {
        1 => CrownType.Clear,
        2 => CrownType.Gold,
        3 => CrownType.Dondaful,
        _ => CrownType.None
    };

}
