namespace TaikoLocalServer.Domain.Enums;

/// <summary>What cabinets are told on each heartbeat (Server Management › Status).</summary>
public enum ServerState
{
    Operational = 0,
    /// <summary>Cabinets keep playing and show that maintenance is coming.</summary>
    PreMaintenance = 1,
    /// <summary>Cabinets show the maintenance screen.</summary>
    Maintenance = 2,
    /// <summary>Cabinets get a network error; game requests are refused.</summary>
    Offline = 3,
}
