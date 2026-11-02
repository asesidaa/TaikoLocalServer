using System.Xml.Linq;
using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

public static class Ac15SpecialBaidLoader
{
    public static async Task<IReadOnlyList<Ac15SpecialBaidEntry>> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        var document = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);

        var rows = document.Descendants("info")
            .Select(element => new Ac15SpecialBaidEntry(
                ReadUInt(element, "baid"),
                ReadString(element, "accesscode"),
                ReadString(element, "comments")))
            .ToArray();

        if (rows.Length == 0)
        {
            throw new InvalidDataException($"No special BAID info rows found in {path}");
        }

        var duplicate = rows
            .GroupBy(row => row.Baid)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new InvalidDataException($"Duplicate special BAID {duplicate.Key} in {path}");
        }

        return rows;
    }

    private static string ReadString(XContainer element, string name)
        => element.Element(name)?.Value ?? string.Empty;

    private static uint ReadUInt(XContainer element, string name)
        => uint.TryParse(element.Element(name)?.Value, out var parsed)
            ? parsed
            : throw new InvalidDataException($"Special BAID row is missing numeric {name}.");
}
