namespace TaikoLocalServer.Contracts.AdminApi.Requests;

/// <summary>
/// Extract these games, one after another. <see cref="Uploaded"/>: the ones whose folder was sent from the browser
/// (api/Extract/Upload) instead of read on the server. <see cref="Mods"/> layers Nijiiro's Data_mods;
/// <see cref="DatatablesOnly"/> copies only Nijiiro's datatables.
/// </summary>
public record ExtractRequest(IReadOnlyList<string> Games, bool Mods = true, IReadOnlyList<string>? Uploaded = null, bool DatatablesOnly = false);
