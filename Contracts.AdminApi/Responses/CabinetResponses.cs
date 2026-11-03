namespace TaikoLocalServer.Contracts.AdminApi.Responses;

public record CabinetItem(string ChassisId, string ShopId, string Version, DateTime? FirstSeen, DateTime? LastSeen, bool Online,
    CabinetAccess Access, int RefusedCount, bool OwnSettings, string? DanSet, IReadOnlyList<uint> Movies, string LocationName = "");

/// <summary>Every cabinet, and whether only approved ones may connect (ServerSettings.RestrictChassisIds).</summary>
public record CabinetList(bool Restricted, IReadOnlyList<CabinetItem> Cabinets);

public record CabinetSettingsRequest(bool OwnSettings, string? DanSet, IReadOnlyList<uint> Movies);
