namespace TaikoLocalServer.Domain.Entities;

/// <summary>
/// The item shop season an older game's cabinets show (Green, Blue, Yellow), picked in Server Management › Game data.
/// Without a row, ServerSettings.json's Eras.&lt;Game&gt;.ActiveShopSeasonId is used.
/// </summary>
public class ShopLiveSeason
{
    /// <summary>The game, as GameEra's name.</summary>
    public string Era { get; set; } = string.Empty;
    public uint SeasonId { get; set; }
}
