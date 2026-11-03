namespace TaikoLocalServer.Tests.Extract;

// Which older game a USRDIR/data folder is (the folder picker marks only that game's): the newest config folder in it,
// or for Kimidori and Momoiro (no config folders) their PARAM.SFO title.
public sealed class OlderGameOfTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "taiko-older-game-" + Guid.NewGuid().ToString("N"));

    private string Data(string game, string[] configs, string? sfoTitle = null)
    {
        var data = Path.Combine(root, game, "USRDIR", "data");
        Directory.CreateDirectory(Path.Combine(data, "fumen"));
        foreach (var c in configs) Directory.CreateDirectory(Path.Combine(data, "config", c));
        if (sfoTitle is not null) File.WriteAllText(Path.Combine(root, game, "PARAM.SFO"), $"PSF\0TITLE\0{sfoTitle}\0");
        return data;
    }

    public void Dispose() => Directory.Delete(root, true);

    [Fact]
    public void TheNewestConfig_IsTheGame()
    {
        // Green keeps Blue's and older configs too; Blue keeps the older ones' -7 variants.
        Assert.Equal(GameEra.Green, ExtractRunner.OlderGameOf(Data("green", ["S10100-1", "S11100-1", "ST9100-1", "common"])));
        Assert.Equal(GameEra.Blue, ExtractRunner.OlderGameOf(Data("blue", ["S10100-1", "S10100-7", "ST9100-7", "common"])));
        Assert.Equal(GameEra.Murasaki, ExtractRunner.OlderGameOf(Data("murasaki", ["ST5100-1", "ST6100-1", "common"])));
    }

    [Fact]
    public void AnEarlyRed_WithOnlyItsDash7Config_IsRed() =>
        Assert.Equal(GameEra.Red, ExtractRunner.OlderGameOf(Data("red", ["ST8100-7", "common"])));

    [Fact]
    public void KimidoriAndMomoiro_AreToldApartByTheirTitle()
    {
        Assert.Equal(GameEra.Kimidori, ExtractRunner.OlderGameOf(Data("kimidori", [], "Taiko no Tatsujin(ST51)")));
        Assert.Equal(GameEra.Momoiro, ExtractRunner.OlderGameOf(Data("momoiro", [], "Taiko no Tatsujin(ST41)")));
        Assert.Null(ExtractRunner.OlderGameOf(Data("unknown", [])));
    }
}
