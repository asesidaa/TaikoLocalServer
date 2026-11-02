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
}
