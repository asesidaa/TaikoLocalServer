namespace TaikoWebUI.Shared.Customize;

public sealed record CostumePickerValue(uint CurrentId, IReadOnlyList<uint> UnlockedIds);

public sealed record TitlePickerValue(
    string Title,
    uint TitlePlateId,
    IReadOnlyList<uint> UnlockedTitleIds);

public sealed record NeiroPickerValue(uint CurrentId, IReadOnlyList<uint> UnlockedIds);

public sealed record ColorPickerValue(uint BodyColor, uint FaceColor, uint LimbColor);
