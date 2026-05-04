using System.Text.Json.Serialization;

namespace TaikoLocalServer.Application.Catalog;

public class WordListEntry
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("japaneseText")]
    public string JapaneseText { get; set; } = string.Empty;

    [JsonPropertyName("englishUsText")]
    public string EnglishUsText { get; set; } = string.Empty;

    [JsonPropertyName("chineseTText")]
    public string ChineseTText { get; set; } = string.Empty;

    [JsonPropertyName("koreanText")]
    public string KoreanText { get; set; } = string.Empty;
}