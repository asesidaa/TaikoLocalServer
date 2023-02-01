using System.Collections.Immutable;
using System.Text.Json;
using ICSharpCode.SharpZipLib.GZip;
using Microsoft.Extensions.Options;
using SharedProject.Models;
using SharedProject.Utils;
using Swan.Mapping;
using TaikoLocalServer.Settings;
using TaikoWebUI.Shared.Models;
using Throw;

namespace TaikoLocalServer.Services;

public class GameDataService : IGameDataService
{
    private readonly DataSettings settings;

    private ImmutableDictionary<uint, GetDanOdaiResponse.OdaiData> danDataDictionary =
        ImmutableDictionary<uint, GetDanOdaiResponse.OdaiData>.Empty;

    private ImmutableDictionary<uint, GetfolderResponse.EventfolderData> folderDictionary =
        ImmutableDictionary<uint, GetfolderResponse.EventfolderData>.Empty;

    private ImmutableDictionary<uint, GetSongIntroductionResponse.SongIntroductionData> introDataDictionary =
        ImmutableDictionary<uint, GetSongIntroductionResponse.SongIntroductionData>.Empty;

    private ImmutableDictionary<uint, MusicAttributeEntry> musicAttributes =
        ImmutableDictionary<uint, MusicAttributeEntry>.Empty;

    private List<MusicOrderEntry> musicOrders = new();

    private List<uint> musics = new();

    private List<uint> musicsWithUra = new();

    private List<uint> musicWithGenre17 = new();

    private ImmutableDictionary<uint, GetShopFolderResponse.ShopFolderData> shopFolderDictionary =
        ImmutableDictionary<uint, GetShopFolderResponse.ShopFolderData>.Empty;

    private Dictionary<string, uint> tokenDataDictionary = new();

    public GameDataService(IOptions<DataSettings> settings)
    {
        this.settings = settings.Value;
    }

    public List<uint> GetMusicList()
    {
        return musics;
    }

    public List<uint> GetMusicWithUraList()
    {
        return musicsWithUra;
    }

    public List<uint> GetMusicWithGenre17List()
    {
        return musicWithGenre17;
    }

    public ImmutableDictionary<uint, MusicAttributeEntry> GetMusicAttributes()
    {
        return musicAttributes;
    }

    public ImmutableDictionary<uint, GetDanOdaiResponse.OdaiData> GetDanDataDictionary()
    {
        return danDataDictionary;
    }

    public ImmutableDictionary<uint, GetSongIntroductionResponse.SongIntroductionData> GetSongIntroDictionary()
    {
        return introDataDictionary;
    }

    public ImmutableDictionary<uint, GetfolderResponse.EventfolderData> GetFolderDictionary()
    {
        return folderDictionary;
    }

    public ImmutableDictionary<uint, GetShopFolderResponse.ShopFolderData> GetShopFolderDictionary()
    {
        return shopFolderDictionary;
    }

    public Dictionary<string, uint> GetTokenDataDictionary()
    {
        return tokenDataDictionary;
    }

    public async Task InitializeAsync()
    {
        var dataPath = PathHelper.GetDataPath();
        var musicAttributePath = Path.Combine(dataPath, Constants.MUSIC_ATTRIBUTE_FILE_NAME);
        var compressedMusicAttributePath = Path.Combine(dataPath, Constants.MUSIC_ATTRIBUTE_COMPRESSED_FILE_NAME);
        var musicOrderPath = Path.Combine(dataPath, Constants.MUSIC_ORDER_FILE_NAME);
        var compressedMusicOrderPath = Path.Combine(dataPath, Constants.MUSIC_ORDER_COMPRESSED_FILE_NAME);
        var danDataPath = Path.Combine(dataPath, settings.DanDataFileName);
        var songIntroDataPath = Path.Combine(dataPath, settings.IntroDataFileName);
        var eventFolderDataPath = Path.Combine(dataPath, settings.EventFolderDataFileName);
        var shopFolderDataPath = Path.Combine(dataPath, settings.ShopFolderDataFileName);
        var tokenDataPath = Path.Combine(dataPath, settings.TokenDataFileName);

        if (File.Exists(compressedMusicAttributePath)) TryDecompressMusicAttribute();
        await using var musicAttributeFile = File.OpenRead(musicAttributePath);
        if (File.Exists(compressedMusicOrderPath)) TryDecompressMusicOrder();
        await using var musicOrderFile = File.OpenRead(musicOrderPath);
        await using var danDataFile = File.OpenRead(danDataPath);
        await using var songIntroDataFile = File.OpenRead(songIntroDataPath);
        await using var eventFolderDataFile = File.OpenRead(eventFolderDataPath);
        await using var shopFolderDataFile = File.OpenRead(shopFolderDataPath);
        await using var tokenDataFile = File.OpenRead(tokenDataPath);

        var attributesData = await JsonSerializer.DeserializeAsync<MusicAttributes>(musicAttributeFile);
        var ordersData = await JsonSerializer.DeserializeAsync<MusicOrder>(musicOrderFile);
        var danData = await JsonSerializer.DeserializeAsync<List<DanData>>(danDataFile);
        var introData = await JsonSerializer.DeserializeAsync<List<SongIntroductionData>>(songIntroDataFile);
        var eventFolderData = await JsonSerializer.DeserializeAsync<List<EventFolderData>>(eventFolderDataFile);
        var shopFolderData = await JsonSerializer.DeserializeAsync<List<ShopFolderData>>(shopFolderDataFile);
        var tokenData = await JsonSerializer.DeserializeAsync<Dictionary<string, uint>>(tokenDataFile);

        InitializeMusicAttributes(attributesData);

        InitializeMusicOrders(ordersData);

        InitializeDanData(danData);

        InitializeIntroData(introData);

        InitializeEventFolderData(eventFolderData);

        InitializeShopFolderData(shopFolderData);

        InitializeTokenData(tokenData);
    }

    public List<MusicOrderEntry> GetMusicOrders()
    {
        return musicOrders;
    }

    private static void TryDecompressMusicAttribute()
    {
        var dataPath = PathHelper.GetDataPath();
        var musicAttributePath = Path.Combine(dataPath, Constants.MUSIC_ATTRIBUTE_FILE_NAME);
        var compressedMusicAttributePath = Path.Combine(dataPath, Constants.MUSIC_ATTRIBUTE_COMPRESSED_FILE_NAME);

        using var compressed = File.Open(compressedMusicAttributePath, FileMode.Open);
        using var output = File.Create(musicAttributePath);

        GZip.Decompress(compressed, output, true);
    }

    private static void TryDecompressMusicOrder()
    {
        var dataPath = PathHelper.GetDataPath();
        var musicOrderPath = Path.Combine(dataPath, Constants.MUSIC_ORDER_FILE_NAME);
        var compressedMusicOrderPath = Path.Combine(dataPath, Constants.MUSIC_ORDER_COMPRESSED_FILE_NAME);

        using var compressed = File.Open(compressedMusicOrderPath, FileMode.Open);
        using var output = File.Create(musicOrderPath);

        GZip.Decompress(compressed, output, true);
    }

    private void InitializeIntroData(List<SongIntroductionData>? introData)
    {
        introData.ThrowIfNull("Shouldn't happen!");
        introDataDictionary = introData.ToImmutableDictionary(data => data.SetId, ToResponseIntroData);
    }

    private void InitializeDanData(List<DanData>? danData)
    {
        danData.ThrowIfNull("Shouldn't happen!");
        danDataDictionary = danData.ToImmutableDictionary(data => data.DanId, ToResponseOdaiData);
    }

    private void InitializeEventFolderData(List<EventFolderData>? eventFolderData)
    {
        eventFolderData.ThrowIfNull("Shouldn't happen!");
        folderDictionary = eventFolderData.ToImmutableDictionary(data => data.FolderId, ToResponseEventFolderData);
    }

    private void InitializeShopFolderData(List<ShopFolderData>? shopFolderData)
    {
        shopFolderData.ThrowIfNull("Shouldn't happen!");
        shopFolderDictionary = shopFolderData.ToImmutableDictionary(data => data.SongNo, ToResponseShopFolderData);
    }

    private void InitializeTokenData(Dictionary<string, uint>? tokenData)
    {
        tokenData.ThrowIfNull("Shouldn't happen!");
        tokenDataDictionary = tokenData;
    }

    private void InitializeMusicAttributes(MusicAttributes? attributesData)
    {
        attributesData.ThrowIfNull("Shouldn't happen!");

        musicAttributes = attributesData.MusicAttributeEntries.ToImmutableDictionary(attribute => attribute.MusicId);

        musics = musicAttributes.Select(pair => pair.Key)
            .ToList();
        musics.Sort();

        musicsWithUra = musicAttributes.Where(attribute => attribute.Value.HasUra)
            .Select(pair => pair.Key)
            .ToList();
        musicsWithUra.Sort();
    }

    private void InitializeMusicOrders(MusicOrder? ordersData)
    {
        ordersData.ThrowIfNull("Shouldn't happen!");

        musicOrders = ordersData.Order.ToList();

        musicWithGenre17 = musicOrders.Where(x => x.GenreNo == 17)
            .Select(x => x.SongId)
            .ToList();
        musicWithGenre17.Sort();
    }

    private static GetDanOdaiResponse.OdaiData ToResponseOdaiData(DanData data)
    {
        var responseOdaiData = new GetDanOdaiResponse.OdaiData
        {
            DanId = data.DanId,
            Title = data.Title,
            VerupNo = data.VerupNo
        };

        var odaiSongs =
            data.OdaiSongList.Select(song => song.CopyPropertiesToNew<GetDanOdaiResponse.OdaiData.OdaiSong>());
        responseOdaiData.AryOdaiSongs.AddRange(odaiSongs);

        var odaiBorders =
            data.OdaiBorderList.Select(border => border.CopyPropertiesToNew<GetDanOdaiResponse.OdaiData.OdaiBorder>());
        responseOdaiData.AryOdaiBorders.AddRange(odaiBorders);

        return responseOdaiData;
    }

    private static GetSongIntroductionResponse.SongIntroductionData ToResponseIntroData(SongIntroductionData data)
    {
        var responseOdaiData = new GetSongIntroductionResponse.SongIntroductionData
        {
            SetId = data.SetId,
            VerupNo = data.VerupNo,
            MainSongNo = data.MainSongNo,
            SubSongNoes = data.SubSongNo
        };

        return responseOdaiData;
    }

    private static GetfolderResponse.EventfolderData ToResponseEventFolderData(EventFolderData data)
    {
        var responseEventFolderData = new GetfolderResponse.EventfolderData
        {
            FolderId = data.FolderId,
            VerupNo = data.VerupNo,
            Priority = data.Priority,
            SongNoes = data.SongNo
        };

        return responseEventFolderData;
    }

    private static GetShopFolderResponse.ShopFolderData ToResponseShopFolderData(ShopFolderData data)
    {
        var responseShopFolderData = new GetShopFolderResponse.ShopFolderData
        {
            SongNo = data.SongNo,
            Price = data.Price
        };

        return responseShopFolderData;
    }
}