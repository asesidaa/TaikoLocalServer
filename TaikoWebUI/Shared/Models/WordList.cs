using System.Text.Json.Serialization;

namespace TaikoWebUI.Shared.Models;

public class WordList
{
    [JsonPropertyName("items")] public List<WordListEntry> WordListEntries { get; set; } = new();
}