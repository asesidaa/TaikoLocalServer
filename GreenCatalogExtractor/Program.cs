using System.CommandLine;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor;

var root = new RootCommand("Extract Green customization catalogs from a read-only game-data tree.");
var extract = new Command("extract", "Extract Green costume, title, and tone catalogs.");

var gameDataOption = new Option<string>(
    "--game-data",
    () => Path.Combine("Host", "wwwroot", "data", "green", "data"),
    "Path to the Green USRDIR/data tree.");
var ebootOption = new Option<string?>("--eboot", "Optional EBOOT.ELF or .i64 path used by enrichment stages.");
var wikiOption = new Option<bool>("--wiki", "Enable wiki name enrichment.");
var overridesOption = new Option<string?>("--overrides", "Optional operator overrides JSON path.");
var outOption = new Option<string>(
    "--out",
    () => Path.Combine("Host", "wwwroot", "data", "green"),
    "Output directory for green_*_data.json.");

extract.AddOption(gameDataOption);
extract.AddOption(ebootOption);
extract.AddOption(wikiOption);
extract.AddOption(overridesOption);
extract.AddOption(outOption);
root.AddCommand(extract);

extract.SetHandler(async (gameData, eboot, wiki, overrides, output) =>
{
    await GreenCatalogExtractor.ExtractAsync(
        new GreenExtractorOptions(gameData, output, eboot, wiki, overrides),
        CancellationToken.None);
}, gameDataOption, ebootOption, wikiOption, overridesOption, outOption);

return await root.InvokeAsync(args);
