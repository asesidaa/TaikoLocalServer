namespace TaikoLocalServer.Contracts.AdminApi.ViewModels;

public partial class Neiro
{
    public uint NeiroId { get; set; }

    public string NeiroName { get; init; } = string.Empty;

    public string NeiroNameEN { get; init; } = string.Empty;

    public string NeiroNameCN { get; init; } = string.Empty;

    public string NeiroNameKO { get; init; } = string.Empty;
}
