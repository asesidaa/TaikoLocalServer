namespace TaikoWebUI.Shared.Customize;

public sealed record PlayerPreviewModel(
    string MyDonName,
    string Title,
    uint TitlePlateId,
    uint Kigurumi,
    uint Head,
    uint Body,
    uint Face,
    uint Puchi,
    uint BodyColor,
    uint FaceColor,
    uint LimbColor,
    bool IsDisplayDanOnNamePlate)
{
    public static PlayerPreviewModel FromUserSetting(UserSetting setting)
        => new(
            setting.MyDonName,
            setting.Title,
            setting.TitlePlateId,
            setting.Kigurumi,
            setting.Head,
            setting.Body,
            setting.Face,
            setting.Puchi,
            setting.BodyColor,
            setting.FaceColor,
            setting.LimbColor,
            setting.IsDisplayDanOnNamePlate);

    public static PlayerPreviewModel FromAc15(Ac15ProfileSettingsDto setting)
    {
        var slots = setting.Customization?.CostumeSlots.ToDictionary(slot => slot.Slot, StringComparer.Ordinal)
                    ?? new Dictionary<string, Ac15CostumeSlotDto>(StringComparer.Ordinal);
        var colors = setting.Customization?.Colors;

        return new PlayerPreviewModel(
            setting.Identity.MyDonName,
            setting.Customization?.Title?.TitleText ?? string.Empty,
            setting.Customization?.Title?.TitleId ?? 0,
            GetSlot(slots, "kigurumi"),
            GetSlot(slots, "head"),
            GetSlot(slots, "body"),
            GetSlot(slots, "face"),
            GetSlot(slots, "puchi"),
            colors?.BodyColor ?? 1,
            colors?.FaceColor ?? 0,
            colors?.LimbColor ?? 3,
            setting.Options.NamePlate?.DisplayDanOnNamePlate ?? false);
    }

    private static uint GetSlot(IReadOnlyDictionary<string, Ac15CostumeSlotDto> slots, string slot)
        => slots.TryGetValue(slot, out var value) ? value.CurrentId : 0;
}
