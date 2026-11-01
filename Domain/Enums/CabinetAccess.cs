namespace TaikoLocalServer.Domain.Enums;

/// <summary>Whether a cabinet may connect. Pending: seen while the server only allows approved cabinets.</summary>
public enum CabinetAccess
{
    Allowed = 0,
    Pending = 1,
    Blocked = 2,
}
