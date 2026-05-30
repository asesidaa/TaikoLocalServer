using System.Xml;
using System.Xml.Linq;
using TaikoLocalServer.Application.Catalog.Blue;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

public sealed class BlueBattleDataLoader
{
    private static readonly BattleFileDefinition[] FileDefinitions =
    [
        new("battleadjsetting.xml", "adjustedsetting"),
        new("battlenpcinfo.xml", "npcinfo"),
        new("battlestageinfo.xml", "stageinfo"),
        new("battlesupportinfo.xml", "supportinfo"),
        new("battletokeninfo.xml", "tokeninfo")
    ];

    public Task<BlueBattleCatalog> LoadAsync(CancellationToken cancellationToken)
        => LoadFromDirectoryAsync(BlueGameDataPaths.BattleRoot, cancellationToken);

    public static async Task<BlueBattleCatalog> LoadFromDirectoryAsync(
        string battleRoot,
        CancellationToken cancellationToken)
    {
        var files = new List<BlueBattleCatalogFile>(FileDefinitions.Length);
        foreach (var definition in FileDefinitions)
        {
            files.Add(await LoadFileAsync(battleRoot, definition, cancellationToken));
        }

        return new BlueBattleCatalog
        {
            IsRawDataAvailable = files.All(file => file.IsPresent && file.IsXmlParsed),
            EnablesBattleAdvertisement = false,
            Files = files
        };
    }

    private static async Task<BlueBattleCatalogFile> LoadFileAsync(
        string battleRoot,
        BattleFileDefinition definition,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(battleRoot, definition.FileName);
        if (!File.Exists(path))
        {
            return new BlueBattleCatalogFile
            {
                FileName = definition.FileName,
                IsPresent = false,
                IsXmlParsed = false,
                ElementCount = 0,
                RowCount = 0
            };
        }

        try
        {
            await using var stream = File.OpenRead(path);
            var document = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);
            return new BlueBattleCatalogFile
            {
                FileName = definition.FileName,
                IsPresent = true,
                IsXmlParsed = true,
                ElementCount = document.Descendants().Count(),
                RowCount = document.Descendants()
                    .Count(element => element.Name.LocalName == definition.RowElementName)
            };
        }
        catch (XmlException ex)
        {
            return new BlueBattleCatalogFile
            {
                FileName = definition.FileName,
                IsPresent = true,
                IsXmlParsed = false,
                ParseError = ex.Message,
                ElementCount = 0,
                RowCount = 0
            };
        }
    }

    private sealed record BattleFileDefinition(string FileName, string RowElementName);
}
