using TaikoLocalServer.Contracts.AdminApi.Ac15ProfileSettings;

namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15ProfileCapabilities(
    IReadOnlyList<string> CostumeSlots,
    bool SupportsTitle,
    bool SupportsTone,
    bool SupportsColors,
    bool SupportsDisplayDanOnNamePlate,
    bool SupportsFolderCloseButton,
    bool SupportsAutoCostume,
    bool SupportsHowToPlayTutorialFlag,
    bool SupportsLocalRankingDifficulty,
    bool SupportsDefaultSelectedSelfBestDifficulty,
    bool SupportsTaikojukuFolderDan)
{
    public static Ac15ProfileCapabilities CurrentFull { get; } = new(
        CostumeSlots: ["kigurumi", "head", "body", "face", "puchi"],
        SupportsTitle: true,
        SupportsTone: true,
        SupportsColors: true,
        SupportsDisplayDanOnNamePlate: true,
        SupportsFolderCloseButton: true,
        SupportsAutoCostume: true,
        SupportsHowToPlayTutorialFlag: true,
        SupportsLocalRankingDifficulty: true,
        SupportsDefaultSelectedSelfBestDifficulty: true,
        SupportsTaikojukuFolderDan: true);

    public Ac15ProfileCapabilitiesDto ToDto()
        => new(
            CostumeSlots,
            SupportsTitle,
            SupportsTone,
            SupportsColors,
            SupportsDisplayDanOnNamePlate,
            SupportsFolderCloseButton,
            SupportsAutoCostume,
            SupportsHowToPlayTutorialFlag,
            SupportsLocalRankingDifficulty,
            SupportsDefaultSelectedSelfBestDifficulty,
            SupportsTaikojukuFolderDan);
}
