using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Domain.Entities;

/// <summary>A cabinet the server has seen (every game request carries its chassis ID), or one an admin pre-approved.</summary>
public class Cabinet
{
    public string ChassisId { get; set; } = string.Empty;
    public string ShopId { get; set; } = string.Empty;
    /// <summary>The location name it reports at start-up (Garm RegisterSystemBoard), e.g. its loader's shop name.</summary>
    public string LocationName { get; set; } = string.Empty;
    /// <summary>The game version it last spoke: "WW 39.06" or "CHN".</summary>
    public string Version { get; set; } = string.Empty;
    public DateTime? FirstSeen { get; set; }
    public DateTime? LastSeen { get; set; }
    public CabinetAccess Access { get; set; }
    /// <summary>Requests refused while it was blocked or waiting for approval.</summary>
    public int RefusedCount { get; set; }

    /// <summary>Off: it uses the defaults (the default Dani Dojo set, movie_data.json).</summary>
    public bool OwnSettings { get; set; }
    /// <summary>A Dani Dojo course set id (null: the default set).</summary>
    public string? DanSet { get; set; }
    /// <summary>Movie ids it shows, comma-separated (only with OwnSettings).</summary>
    public string Movies { get; set; } = string.Empty;
}
