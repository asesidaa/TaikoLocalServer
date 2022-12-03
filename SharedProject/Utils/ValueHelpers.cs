namespace SharedProject.Utils;

public static class ValueHelpers
{
    public static T Min<T>(T a, T b) where T : IComparable
    {
        return Comparer<T>.Default.Compare(a, b) <= 0 ? a : b;
    }

    public static T Max<T>(T a, T b) where T : IComparable
    {
        return Comparer<T>.Default.Compare(a, b) >= 0 ? a : b;
    }
}