namespace TaikoLocalServer.Contracts.AdminApi.ViewModels;

public partial class Title
{
    public uint TitleId { get; set; }

    public string TitleName { get; init; } = string.Empty;

    public string TitleNameEN { get; init; } = string.Empty;

    public string TitleNameCN { get; init; } = string.Empty;

    public string TitleNameKO { get; init; } = string.Empty;

    public uint TitleRarity { get; init; }

    public override bool Equals(object? obj)
    {
        if (obj is Title title)
        {
            return title.TitleName.Equals(TitleName);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return TitleName.GetHashCode();
    }
}
