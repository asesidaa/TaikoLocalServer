using TaikoLocalServer.Contracts.AdminApi.ViewModels;

namespace TaikoWebUI.Shared.Customize;

public enum TitleSelectionMode
{
    TitlePlate,
    TitleId
}

public static class TitlePickerCatalog
{
    public static IReadOnlyList<uint> GetSelectableIds(
        IReadOnlyDictionary<uint, Title> catalog,
        uint currentId,
        TitleSelectionMode selectionMode)
    {
        var ids = selectionMode == TitleSelectionMode.TitleId
            ? catalog.Values.Select(title => title.TitleId)
            : catalog.Values.Select(title => title.TitleRarity);

        return ids
            .Append(currentId)
            .Distinct()
            .OrderBy(id => id)
            .ToArray();
    }

    public static string ResolveSelectedTitleText(
        IReadOnlyDictionary<uint, Title> catalog,
        uint selectedId,
        string fallback)
    {
        return catalog.TryGetValue(selectedId, out var title)
            ? DisplayTitleText(title)
            : fallback;
    }

    public static string DisplayTitleText(Title title)
        => string.IsNullOrWhiteSpace(title.TitleName)
            ? $"#{title.TitleId:D3}"
            : title.TitleName;
}
