using System.Text.Json.Serialization;

namespace TaikoLocalServer.Application.Catalog;

public class WordList
{
    [JsonPropertyName("items")]
    public List<WordListEntry> WordListEntries { get; set; } = new();
}