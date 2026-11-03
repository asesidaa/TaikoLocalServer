namespace TaikoLocalServer.Application.Abstractions;

/// <summary>
/// Cabinets as they connect, from memory (it runs on every game request), written through to the database.
/// </summary>
public interface ICabinetRegistry
{
    Task InitializeAsync();

    /// <summary>Records a request from <paramref name="chassisId"/>; false when it must be refused (blocked, or not approved).</summary>
    bool Seen(string chassisId, string shopId, string version);

    /// <summary>Records the location name a cabinet reports at start-up. A cabinet not seen yet gets it on its first
    /// game request (this never adds one, so it can't bypass approval).</summary>
    void Located(string chassisId, string locationName);

    IReadOnlyList<Cabinet> List();

    Cabinet? Find(string chassisId);

    bool Restricted { get; }

    /// <summary>Changes a cabinet (adding it if it's new, e.g. pre-approved) and saves it.</summary>
    Task UpdateAsync(string chassisId, Action<Cabinet> change, CancellationToken cancellationToken);

    Task<bool> RemoveAsync(string chassisId, CancellationToken cancellationToken);
}
