using System.Collections.Immutable;
using TaikoLocalServer.Contracts.AdminApi.Responses;

namespace TaikoLocalServer.Application.Abstractions;

public interface IGameDataCatalog
{
    public Task InitializeAsync();

    /// <summary>Reads every file again. A file that fails keeps its previous contents; see <see cref="GetFileStatus"/>.</summary>
    public Task ReloadAsync();

    public IReadOnlyList<GameDataFileStatus> GetFileStatus();

    /// <summary>The raw JSON of an editable data file, or null for an unknown name or a missing file.</summary>
    public Task<string?> ReadDataFileAsync(string name);

    /// <summary>
    /// Validates <paramref name="json"/> with the parser the server uses, writes it (temp file + rename, the old file
    /// kept as .bak) and reloads. Returns null on success, else why it was refused.
    /// </summary>
    public Task<string?> SaveDataFileAsync(string name, string json);

    public List<uint> GetMusicList();

    public List<uint> GetMusicWithUraList();

    public ImmutableDictionary<uint, SongIntroductionData> GetSongIntroductionDictionary();

    public ImmutableDictionary<uint, MovieData> GetMovieDataDictionary();

    public ImmutableDictionary<uint, EventFolderData> GetEventFolderDictionary();

    public ImmutableDictionary<uint, TelopData> GetTelopDataDictionary();

    public ImmutableDictionary<uint, DanData> GetCommonDanDataDictionary();

    public ImmutableDictionary<uint, DanData> GetCommonGaidenDataDictionary();

    public List<ShopFolderData> GetShopFolderList();

    public uint GetShopFolderVerup();

    public Dictionary<string, int> GetTokenDataDictionary();

    public List<uint> GetLockedSongsList();

    public List<uint> GetTimeLimitedSongsList();

    public List<uint> GetLockedUraSongsList();

    public Dictionary<uint, MusicDetail> GetMusicDetailDictionary();

    public List<Costume> GetCostumeList();

    public Dictionary<uint, Title> GetTitleDictionary();

    public Dictionary<string, List<uint>> GetLockedCostumeDataDictionary();

    public Dictionary<string, List<uint>> GetLockedTitleDataDictionary();

    public List<int> GetCostumeFlagArraySizes();

    public int GetTitleFlagArraySize();

    public int GetToneFlagArraySize();

    public ImmutableDictionary<string, uint> GetQRCodeDataDictionary();
}
