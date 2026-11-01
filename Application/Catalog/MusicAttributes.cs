using System.Text.Json.Serialization;

namespace TaikoLocalServer.Application.Catalog;

/// <summary>The game's music_attribute datatable, as far as the server reads it.</summary>
public class MusicAttributes
{
    [JsonPropertyName("items")]
    public List<MusicAttributeEntry> Entries { get; set; } = [];
}

public class MusicAttributeEntry
{
    [JsonPropertyName("uniqueId")]
    public uint MusicId { get; set; }

    /// <summary>The game marks the songs of its latest update as new.</summary>
    [JsonPropertyName("new")]
    public bool IsNew { get; set; }
}
