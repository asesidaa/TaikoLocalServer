using System.Xml.Linq;
using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

public static class Ac15PresentLoader
{
    public static async Task<IReadOnlyList<Ac15PresentItem>> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        var document = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);
        var root = document.Root ?? throw new InvalidDataException($"Missing root element in {path}");

        var rows = root.Elements("PresentItemData")
            .Select(element => new Ac15PresentItem(
                ReadUInt(element, "index"),
                ReadUInt(element, "type"),
                ReadUInt(element, "itemNumber"),
                ReadUInt(element, "donPoint")))
            .ToArray();

        if (rows.Length == 0)
        {
            throw new InvalidDataException($"No PresentItemData rows found in {path}");
        }

        var duplicate = rows
            .GroupBy(row => row.Index)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new InvalidDataException($"Duplicate present index {duplicate.Key} in {path}");
        }

        return rows;
    }

    private static uint ReadUInt(XContainer element, string name)
        => uint.TryParse(element.Element(name)?.Value, out var parsed)
            ? parsed
            : throw new InvalidDataException($"Present row is missing numeric {name}.");
}
