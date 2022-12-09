namespace TaikoWebUI.Shared.Models;

public class Title
{
    public int TitleId { get; set; }

    public string TitleName { get; init; } = string.Empty;

    public override bool Equals(object? obj)
    {
        if (obj is Title title) return title.TitleName.Equals(TitleName);

        return false;
    }

    public override int GetHashCode()
    {
        return TitleName.GetHashCode();
    }
}