namespace TaikoLocalServer.Contracts.AdminApi.ViewModels;

public class ScoreFacet
{
    public string Label { get; set; } = string.Empty;

    public uint BestScore { get; set; }

    public uint BestRate { get; set; }

    public CrownType BestCrown { get; set; }
}
