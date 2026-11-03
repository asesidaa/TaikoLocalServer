namespace TaikoLocalServer.Contracts.AdminApi.ViewModels;

/// <summary>A player's card look (every game's profile shows the same card). On save, a null field is left as it was.</summary>
public class CardLook
{
    /// <summary>A game palette index (0 = Don red).</summary>
    public uint? Color { get; set; }
    public CardPattern? Pattern { get; set; }
    /// <summary>A game palette index, or -1 for none.</summary>
    public int? Fade { get; set; }
    public CardAnimation? Animation { get; set; }
}
