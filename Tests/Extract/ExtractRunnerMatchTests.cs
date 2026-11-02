using TaikoLocalServer.Contracts.AdminApi.Responses;
namespace TaikoLocalServer.Tests.Extract;

// The game files an admin may send from the browser (Infrastructure/GameDataCatalog/ExtractRunner.Match): false = keep
// the contents, true = name only (movies), null = refused.
public sealed class ExtractRunnerMatchTests
{
    private static readonly ExtractInputs Inputs = new(["Data/x64/datatable/musicinfo.bin", "Data/x64/model/body/"], ["Data/x64/movie/"],
        new ExtractInputs(["Data_mods/x64/model/body/"], ["Data_mods/x64/movie/"], null));

    [Theory]
    [InlineData("Data/x64/datatable/musicinfo.bin")]
    [InlineData("Data/x64/model/body/a/b.numdlb")]
    [InlineData("Data_mods/x64/model/body/a.nutexb")]
    public void ListedFiles_KeepTheirContents(string path) => Assert.False(ExtractRunner.Match(Inputs, path));

    [Fact]
    public void Movie_ByNameOnly() => Assert.True(ExtractRunner.Match(Inputs, "Data/x64/movie/mv_01.usm"));

    // An older game's title names sit in nutdata/<pack>/<nn>/title_name/ (extract --inputs --era).
    private static readonly ExtractInputs Ac15 = new(["data/config/*/musicinfo.xml"], ["data/nutdata/*/*/title_name/"], null);

    [Theory]
    [InlineData("data/config/ST8100-7/musicinfo.xml", false)]
    [InlineData("data/nutdata/pack/05/title_name/title_1_9.nut", true)]
    [InlineData("data/config/musicinfo.xml", null)]
    [InlineData("data/nutdata/pack/title_name/a.nut", null)]
    [InlineData("data/nutdata/pack/05/title_name/x/a.nut", null)]
    public void Wildcard_MatchesOneFolder(string path, bool? expected) => Assert.Equal(expected, ExtractRunner.Match(Ac15, path));

    [Theory]
    [InlineData("Data/x64/movie/x/y.usm")]
    [InlineData("Data/x64/datatable/other.bin")]
    [InlineData("Data/x64/datatable/musicinfo.bin.exe")]
    [InlineData("Data/x64/model/body/../../../../evil.dll")]
    [InlineData(@"Data/x64/model/body/..\..\evil")]
    [InlineData("C:/Data/x64/model/body/a")]
    [InlineData("/Data/x64/model/body/a")]
    [InlineData("Data/x64/model/body//a")]
    public void Others_Refused(string path) => Assert.Null(ExtractRunner.Match(Inputs, path));

    // extract's report lines, read live for the Games tab's progress; an older game's labels lose its name, as in extract.json.
    [Theory]
    [InlineData("red server files: 1440 written, 0 already up to date, 0 missing, 0 failed", "Red", "server files", 1440, 0, 0, 0)]
    [InlineData("momoiro body models: 3 written, 30 already up to date, 1 missing, 2 failed", "Momoiro", "body models", 3, 30, 1, 2)]
    [InlineData("Datatables: 6 written, 0 already up to date, 0 missing, 0 failed", "Nijiiro", "Datatables", 6, 0, 0, 0)]
    public void ParseStep_ReadsAReportLine(string line, string game, string label, int written, int upToDate, int missing, int failed) =>
        Assert.Equal(new ExtractStep(label, written, upToDate, missing, failed), ExtractRunner.ParseStep(line, Enum.Parse<TaikoLocalServer.Domain.Enums.GameEra>(game)));

    [Theory]
    [InlineData("  MISSING  data/config/ST8100-1/musicinfo.xml")]
    [InlineData("game:   D:/RPCS3/Taiko (red)")]
    [InlineData("")]
    public void ParseStep_IgnoresOtherLines(string line) => Assert.Null(ExtractRunner.ParseStep(line, TaikoLocalServer.Domain.Enums.GameEra.Red));
}
