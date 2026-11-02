using System.Collections.Immutable;
using TaikoLocalServer.Contracts.AdminApi.Responses;

namespace TaikoLocalServer.Application.Abstractions;

public interface INijiiroCatalog : IEraGameDataCatalog
{
    /// <summary>Reads every file again. A file that fails keeps its previous contents; see <see cref="GetFileStatus"/>.</summary>
    Task ReloadAsync();

    IReadOnlyList<GameDataFileStatus> GetFileStatus();

    /// <summary>The raw JSON of an editable data file, or null for an unknown name or a missing file.</summary>
    Task<string?> ReadDataFileAsync(string name);

    /// <summary>
    /// Validates <paramref name="json"/> with the parser the server uses, writes it (temp file + rename, the old file
    /// kept as .bak) and reloads. Returns null on success, else why it was refused.
    /// </summary>
    Task<string?> SaveDataFileAsync(string name, string json);

    // ---- Dani Dojo course sets: GetCommonDanDataDictionary is the default set ----

    IReadOnlyList<DanSetInfo> GetDanSets();

    /// <summary>A course set's courses; null or an unknown id gives the default set.</summary>
    ImmutableDictionary<uint, DanData> GetDanSet(string? id);

    /// <summary>A new set, a copy of <paramref name="copyFrom"/>; returns its id.</summary>
    Task<string> CreateDanSetAsync(string name, string copyFrom);

    Task<bool> RenameDanSetAsync(string id, string name);

    Task<bool> SetDefaultDanSetAsync(string id);

    /// <summary>False for the built-in set, the default set, or an unknown id.</summary>
    Task<bool> DeleteDanSetAsync(string id);

    ImmutableDictionary<uint, TelopData> GetTelopDataDictionary();

    List<uint> GetMusicList();

    List<uint> GetMusicWithUraList();

    ImmutableDictionary<uint, SongIntroductionData> GetSongIntroductionDictionary();

    ImmutableDictionary<uint, MovieData> GetMovieDataDictionary();

    ImmutableDictionary<uint, EventFolderData> GetEventFolderDictionary();

    ImmutableDictionary<uint, DanData> GetCommonDanDataDictionary();

    ImmutableDictionary<uint, DanData> GetCommonGaidenDataDictionary();

    List<ShopFolderData> GetShopFolderList();

    uint GetShopFolderVerup();

    Dictionary<string, int> GetTokenDataDictionary();

    List<uint> GetLockedSongsList();

    List<uint> GetTimeLimitedSongsList();

    List<uint> GetLockedUraSongsList();

    Dictionary<uint, MusicDetail> GetMusicDetailDictionary();

    List<Costume> GetCostumeList();

    Dictionary<uint, Title> GetTitleDictionary();

    Dictionary<uint, Neiro> GetNeiroDictionary();

    Dictionary<string, List<uint>> GetLockedCostumeDataDictionary();

    Dictionary<string, List<uint>> GetLockedTitleDataDictionary();

    List<int> GetCostumeFlagArraySizes();

    int GetTitleFlagArraySize();

    int GetToneFlagArraySize();

    ImmutableDictionary<string, uint> GetQRCodeDataDictionary();
}
