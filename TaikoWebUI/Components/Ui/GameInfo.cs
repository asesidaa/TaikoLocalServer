namespace TaikoWebUI.Components.Ui;

/// <summary>The games the server can run (ServerSettings.json's Eras), newest first, with the dot that marks each one.</summary>
public sealed record GameInfo(string Name, string Japanese, string Dot)
{
    public static readonly IReadOnlyList<GameInfo> All =
    [
        new("Nijiiro", "ニジイロ", "conic-gradient(#f84828,#fec000,#1dc83b,#42b8c7,#7838d8,#f84828)"),
        new("Green", "グリーン", "#22c55e"),
        new("Blue", "ブルー", "#3b82f6"),
        new("Yellow", "イエロー", "#eab308"),
        new("Red", "レッド", "#ef4444"),
        new("White", "ホワイト", "#f4f4f5"),
        new("Murasaki", "むらさき", "#8b5cf6"),
        new("Kimidori", "きみどり", "#84cc16"),
        new("Momoiro", "ももいろ", "#f472b6"),
    ];

    public static GameInfo Of(string name) =>
        All.FirstOrDefault(g => string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase)) ?? new(name, "", "#a1a1aa");
}
