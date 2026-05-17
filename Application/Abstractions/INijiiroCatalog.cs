using System.Collections.Immutable;

namespace TaikoLocalServer.Application.Abstractions;

public interface INijiiroCatalog : IEraGameDataCatalog
{
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
