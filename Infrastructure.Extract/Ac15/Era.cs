namespace TaikoLocalServer.Infrastructure.Extract.Ac15;

/// <summary>The older (AC15, PS3-based) games.</summary>
public enum Era
{
    Green,
    Blue,
    Yellow,
    Red,
    White,
    Murasaki,
    Kimidori,
    Momoiro,
}

public static class EraInfo
{
    /// <summary>The folder name under <c>data/</c> and <c>game_data/</c> (<c>PathHelper.GetDataPath(era)</c>).</summary>
    public static string Name(this Era era) => era.ToString().ToLowerInvariant();

    public static bool TryParse(string name, out Era era) => Enum.TryParse(name, ignoreCase: true, out era) && Enum.IsDefined(era);

    /// <summary>The <c>config/&lt;id&gt;</c> folder the server reads (<c>*GameDataPaths.cs</c>). Kimidori and Momoiro keep it at
    /// the top of <c>data</c>.</summary>
    public static string? Config(this Era era) => era switch
    {
        Era.Green => "S11100-1",
        Era.Blue => "S10100-1",
        Era.Yellow => "ST9100-1",
        Era.Red => "ST8100-1",
        Era.White => "ST7100-1",
        Era.Murasaki => "ST6100-1",
        _ => null,
    };

    /// <summary>Files in the config folder: (name, required by <c>*RequiredDataFiles.cs</c>).</summary>
    public static IReadOnlyList<(string Name, bool Required)> ConfigFiles(this Era era)
    {
        List<(string, bool)> files = [("musicinfo.xml", true), ("musicmedleyinfo.xml", true)];
        switch (era)
        {
            case Era.Green: files.Add(("rewardtitlefiltering.xml", true)); break;
            case Era.Yellow or Era.Red or Era.Momoiro: files.Add(("defmusic.bin", true)); break;
            case Era.White or Era.Murasaki: files.AddRange([("defmusic.bin", true), ("present.xml", true), ("spacialbaid.xml", true)]); break;
            case Era.Kimidori: files.AddRange([("defmusic.bin", true), ("present.xml", false), ("spacialbaid.xml", false)]); break;
        }
        return files;
    }
}
