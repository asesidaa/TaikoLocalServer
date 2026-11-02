namespace TaikoLocalServer.Application.Common;

public static class Ac15Difficulty
{
    private static readonly Difficulty[] Ordered =
    [
        Difficulty.Easy,
        Difficulty.Normal,
        Difficulty.Hard,
        Difficulty.Oni,
        Difficulty.UraOni
    ];

    public static Difficulty FromProtocol(uint value) => value switch
    {
        1 => Difficulty.Easy,
        2 => Difficulty.Normal,
        3 => Difficulty.Hard,
        4 => Difficulty.Oni,
        5 => Difficulty.UraOni,
        _ => Difficulty.None
    };

    public static Difficulty FromZeroBasedCourse(uint value) => value switch
    {
        0 => Difficulty.Easy,
        1 => Difficulty.Normal,
        2 => Difficulty.Hard,
        3 => Difficulty.Oni,
        4 => Difficulty.UraOni,
        _ => Difficulty.None
    };

    public static uint ToProtocol(Difficulty difficulty) => (uint)difficulty;

    public static bool IsInRange(Difficulty difficulty, Difficulty min, Difficulty max)
        => difficulty >= min && difficulty <= max;

    public static Difficulty FromSequentialIndex(int index, Difficulty min, Difficulty max)
    {
        var values = Ordered.Where(value => IsInRange(value, min, max)).ToArray();
        if (values.Length == 0)
        {
            return Difficulty.None;
        }

        return values[Math.Clamp(index, 0, values.Length - 1)];
    }

    public static IEnumerable<Difficulty> Range(Difficulty min, Difficulty max)
        => Ordered.Where(value => IsInRange(value, min, max));
}
