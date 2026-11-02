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
        var loadedFiles = new List<LoadedBattleFile>(FileDefinitions.Length);
        foreach (var definition in FileDefinitions)
        {
            loadedFiles.Add(await LoadFileAsync(battleRoot, definition, cancellationToken));
        }

        var files = loadedFiles.Select(file => file.File).ToList();
        var isRawDataAvailable = files.All(file => file.IsPresent && file.IsXmlParsed);

        return new BlueBattleCatalog
        {
            IsRawDataAvailable = isRawDataAvailable,
            EnablesBattleAdvertisement = isRawDataAvailable,
            BattleNpcIds = loadedFiles
                .SelectMany(file => file.BattleNpcs.Select(npc => npc.NpcId))
                .Distinct()
                .Order()
                .ToList(),
            BattleNpcs = loadedFiles
                .SelectMany(file => file.BattleNpcs)
                .GroupBy(npc => npc.NpcId)
                .Select(group => group.First())
                .OrderBy(npc => npc.NpcId)
                .ToList(),
            ReleaseBattleStageIds = loadedFiles
                .SelectMany(file => file.ReleaseBattleStageIds)
                .Distinct()
                .Order()
                .ToList(),
            ReleaseBattleSpecialIds = loadedFiles
                .SelectMany(file => file.ReleaseBattleSpecialIds)
                .Distinct()
                .Order()
                .ToList(),
            BattleBondsLvCap = loadedFiles
                .Select(file => file.BattleBondsLvCap)
                .FirstOrDefault(value => value is not null),
            Files = files
        };
    }

    private static async Task<LoadedBattleFile> LoadFileAsync(
        string battleRoot,
        BattleFileDefinition definition,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(battleRoot, definition.FileName);
        if (!File.Exists(path))
        {
            return new LoadedBattleFile(
                new BlueBattleCatalogFile
                {
                    FileName = definition.FileName,
                    IsPresent = false,
                    IsXmlParsed = false,
                    ElementCount = 0,
                    RowCount = 0
                },
                [],
                [],
                [],
                null);
        }

        try
        {
            await using var stream = File.OpenRead(path);
            var document = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);
            return new LoadedBattleFile(
                new BlueBattleCatalogFile
                {
                    FileName = definition.FileName,
                    IsPresent = true,
                    IsXmlParsed = true,
                    ElementCount = document.Descendants().Count(),
                    RowCount = document.Descendants()
                        .Count(element => element.Name.LocalName == definition.RowElementName)
                },
                definition.FileName == "battlenpcinfo.xml" ? ReadNpcRows(document) : [],
                definition.FileName == "battlestageinfo.xml" ? ReadStageIds(document) : [],
                definition.FileName switch
                {
                    "battletokeninfo.xml" => ReadTokenRewardSpecialIds(document),
                    "battlesupportinfo.xml" => ReadSupportSpecialIds(document),
                    _ => []
                },
                definition.FileName == "battlenpcinfo.xml" ? ReadBattleBondsLvCap(document) : null);
        }
        catch (Exception ex) when (ex is XmlException or InvalidDataException)
        {
            return new LoadedBattleFile(
                new BlueBattleCatalogFile
                {
                    FileName = definition.FileName,
                    IsPresent = true,
                    IsXmlParsed = false,
                    ParseError = ex.Message,
                    ElementCount = 0,
                    RowCount = 0
                },
                [],
                [],
                [],
                null);
        }
    }

    private static IReadOnlyList<uint> ReadStageIds(XDocument document)
        => document.Descendants()
            .Where(element => element.Name.LocalName == "stageinfo")
            .Select(element => ReadRequiredId(element, "stageinfo"))
            .ToList();

    private static IReadOnlyList<BlueBattleNpcCatalogEntry> ReadNpcRows(XDocument document)
        => document.Descendants()
            .Where(element => element.Name.LocalName == "npcinfo")
            .Select(element => new BlueBattleNpcCatalogEntry
            {
                NpcId = ToRuntimeNpcId(ReadRequiredId(element, "npcinfo")),
                StartExp = ReadOptionalUInt32(element, "start_exp") ?? 0,
                InitialDpn = ReadOptionalUInt32(element, "atk") ?? 0
            })
            .ToList();

    private static IReadOnlyList<uint> ReadTokenRewardSpecialIds(XDocument document)
        => document.Descendants()
            .Where(element => element.Name.LocalName == "reward")
            .Select(element => ReadRequiredId(element, "reward"))
            .ToList();

    private static IReadOnlyList<uint> ReadSupportSpecialIds(XDocument document)
        => document.Descendants()
            .Where(element => element.Name.LocalName == "supportinfo")
            .Descendants()
            .Where(element => element.Name.LocalName == "item" && !element.HasElements)
            .Select(element => element.Value)
            .Select(value => uint.TryParse(value, out var parsed) ? parsed : 0)
            .Where(id => id > 0)
            .Distinct()
            .ToList();

    private static uint ReadBattleBondsLvCap(XDocument document)
        => (uint)document.Descendants()
            .Count(element => element.Name.LocalName is "requred_exp" or "required_exp");

    private static uint ReadRequiredId(XElement element, string rowName)
    {
        var idValue = element.Attribute("id")?.Value
            ?? element.Elements().FirstOrDefault(child => child.Name.LocalName == "id")?.Value;
        if (string.IsNullOrWhiteSpace(idValue))
        {
            throw new InvalidDataException($"Blue battle {rowName} row is missing id.");
        }

        if (!uint.TryParse(idValue, out var id))
        {
            throw new InvalidDataException($"Blue battle {rowName} row has invalid id '{idValue}'.");
        }

        return id;
    }

    private static uint ToRuntimeNpcId(uint xmlNpcId)
        => xmlNpcId == 0 ? 0 : xmlNpcId - 1;

    private static uint? ReadOptionalUInt32(XElement element, string childName)
    {
        var value = element.Elements().FirstOrDefault(child => child.Name.LocalName == childName)?.Value;
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!uint.TryParse(value, out var parsed))
        {
            throw new InvalidDataException($"Blue battle npcinfo row has invalid {childName} '{value}'.");
        }

        return parsed;
    }

    private sealed record LoadedBattleFile(
        BlueBattleCatalogFile File,
        IReadOnlyList<BlueBattleNpcCatalogEntry> BattleNpcs,
        IReadOnlyList<uint> ReleaseBattleStageIds,
        IReadOnlyList<uint> ReleaseBattleSpecialIds,
        uint? BattleBondsLvCap);

    private sealed record BattleFileDefinition(string FileName, string RowElementName);
}
