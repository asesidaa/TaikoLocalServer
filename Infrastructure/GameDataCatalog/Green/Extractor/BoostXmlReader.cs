using System.Xml.Linq;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor;

public static class BoostXmlReader
{
    public static async Task<IReadOnlyList<uint>> ReadRewardTitleIdsAsync(
        string path,
        CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        var document = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);

        return document
            .Descendants("rewardtitle")
            .Select(element => uint.TryParse(element.Value, out var id) ? id : (uint?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .OrderBy(id => id)
            .ToArray();
    }
}
