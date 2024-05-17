using Microsoft.Extensions.Options;
using SharedProject.Models;
using SharedProject.Utils;
using System.Collections.Immutable;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using TaikoLocalServer.Settings;
using Throw;

namespace TaikoLocalServer.Services;

public class GameDataService : IGameDataService
{
    private ImmutableDictionary<uint, DanData> commonDanDataDictionary =
        ImmutableDictionary<uint, DanData>.Empty;

    private ImmutableDictionary<uint, DanData> commonGaidenDataDictionary =
        ImmutableDictionary<uint, DanData>.Empty;

    private ImmutableDictionary<uint, MusicInfoEntry> musicInfoes =
        ImmutableDictionary<uint, MusicInfoEntry>.Empty;
    
    private ImmutableDictionary<uint, MovieData> movieDataDictionary =
        ImmutableDictionary<uint, MovieData>.Empty;

    private ImmutableDictionary<uint, SongIntroductionData> songIntroductionDictionary =
        ImmutableDictionary<uint, SongIntroductionData>.Empty;
    
    private ImmutableDictionary<string, uint> qrCodeDataDictionary = ImmutableDictionary<string, uint>.Empty;
    
    private ImmutableDictionary<uint, EventFolderData> eventFolderDictionary = 
        ImmutableDictionary<uint, EventFolderData>.Empty;

    private List<ShopFolderData> shopFolderList = new();

    private List<uint> musics = new();

    private List<uint> musicsWithUra = new();

    private List<uint> lockedSongsList = new();

    private List<int> costumeFlagArraySizes = new();

    private int titleFlagArraySize;

    private int toneFlagArraySize;

    private Dictionary<string, int> tokenDataDictionary = new();

    private readonly DataSettings settings;

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

    public ImmutableDictionary<uint, MovieData> GetMovieDataDictionary()
    {
        return movieDataDictionary;
    }

    public ImmutableDictionary<uint, EventFolderData> GetEventFolderDictionary()
    {
        return eventFolderDictionary;
    }

    public ImmutableDictionary<uint, DanData> GetCommonDanDataDictionary()
    {
        return commonDanDataDictionary;
    }

    public ImmutableDictionary<uint, DanData> GetCommonGaidenDataDictionary()
    {
        return commonGaidenDataDictionary;
    }
    
    public ImmutableDictionary<uint, SongIntroductionData> GetSongIntroductionDictionary()
    {
        return songIntroductionDictionary;
    }

    public List<ShopFolderData> GetShopFolderList()
    {
        return shopFolderList;
    }

    public Dictionary<string, int> GetTokenDataDictionary()
    {
        return tokenDataDictionary;
    }

    public List<uint> GetLockedSongsList()
    {
        return lockedSongsList;
    }

    public List<int> GetCostumeFlagArraySizes()
    {
        return costumeFlagArraySizes;
    }

    public int GetTitleFlagArraySize()
    {
        return titleFlagArraySize;
    }

    public int GetToneFlagArraySize()
    {
        return toneFlagArraySize;
    }

    public ImmutableDictionary<string, uint> GetQRCodeDataDictionary()
    {
        return qrCodeDataDictionary;
    }

    public async Task InitializeAsync()
    {
        var dataPath = PathHelper.GetDataPath();
        var datatablePath = PathHelper.GetDatatablePath();

        var musicInfoPath = Path.Combine(datatablePath, $"{Constants.MUSIC_INFO_BASE_NAME}.json");
        var encryptedInfo = Path.Combine(datatablePath, $"{Constants.MUSIC_INFO_BASE_NAME}.bin");

        var wordlistPath = Path.Combine(datatablePath, $"{Constants.WORDLIST_BASE_NAME}.json");
        var encryptedWordlist = Path.Combine(datatablePath, $"{Constants.WORDLIST_BASE_NAME}.bin");

        var musicOrderPath = Path.Combine(datatablePath, $"{Constants.MUSIC_ORDER_BASE_NAME}.json");
        var encryptedMusicOrder = Path.Combine(datatablePath, $"{Constants.MUSIC_ORDER_BASE_NAME}.bin");

        var donCosRewardPath = Path.Combine(datatablePath, $"{Constants.DON_COS_REWARD_BASE_NAME}.json");
        var encryptedDonCosReward = Path.Combine(datatablePath, $"{Constants.DON_COS_REWARD_BASE_NAME}.bin");

        var shougouPath = Path.Combine(datatablePath, $"{Constants.SHOUGOU_BASE_NAME}.json");
        var encryptedShougou = Path.Combine(datatablePath, $"{Constants.SHOUGOU_BASE_NAME}.bin");

        var neiroPath = Path.Combine(datatablePath, $"{Constants.NEIRO_BASE_NAME}.json");
        var encryptedNeiro = Path.Combine(datatablePath, $"{Constants.NEIRO_BASE_NAME}.bin");

        var danDataPath = Path.Combine(dataPath, settings.DanDataFileName);
        var gaidenDataPath = Path.Combine(dataPath, settings.GaidenDataFileName);
        var songIntroDataPath = Path.Combine(dataPath, settings.IntroDataFileName);
        var movieDataPath = Path.Combine(dataPath, settings.MovieDataFileName);
        var eventFolderDataPath = Path.Combine(dataPath, settings.EventFolderDataFileName);
        var shopFolderDataPath = Path.Combine(dataPath, settings.ShopFolderDataFileName);
        var tokenDataPath = Path.Combine(dataPath, settings.TokenDataFileName);
        var lockedSongsDataPath = Path.Combine(dataPath, settings.LockedSongsDataFileName);
        var qrCodeDataPath = Path.Combine(dataPath, settings.QrCodeDataFileName);

        var encryptedFiles = new List<string>
        {
            encryptedInfo,
            encryptedWordlist,
            encryptedMusicOrder,
            encryptedDonCosReward,
            encryptedShougou,
            encryptedNeiro
        };

        var outputPaths = new List<string>
        {
            musicInfoPath,
            wordlistPath,
            musicOrderPath,
            donCosRewardPath,
            shougouPath,
            neiroPath
        };

        for (var i = 0; i < encryptedFiles.Count; i++)
        {
            if (File.Exists(encryptedFiles[i]))
            {
                DecryptDataTable(encryptedFiles[i], outputPaths[i]);
            }
        }

        foreach (var filePath in outputPaths)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"{Path.GetFileName(filePath)} file not found!");
            }
        }

        await using var musicInfoFile = File.OpenRead(musicInfoPath);
        await using var danDataFile = File.OpenRead(danDataPath);
        await using var gaidenDataFile = File.OpenRead(gaidenDataPath);
        await using var songIntroDataFile = File.OpenRead(songIntroDataPath);
        await using var movieDataFile = File.OpenRead(movieDataPath);
        await using var eventFolderDataFile = File.OpenRead(eventFolderDataPath);
        await using var shopFolderDataFile = File.OpenRead(shopFolderDataPath);
        await using var tokenDataFile = File.OpenRead(tokenDataPath);
        await using var lockedSongsDataFile = File.OpenRead(lockedSongsDataPath);
        await using var donCosRewardFile = File.OpenRead(donCosRewardPath);
        await using var shougouFile = File.OpenRead(shougouPath);
        await using var neiroFile = File.OpenRead(neiroPath);
        await using var qrCodeDataFile = File.OpenRead(qrCodeDataPath);

        var infosData = await JsonSerializer.DeserializeAsync<MusicInfos>(musicInfoFile);
        var danData = await JsonSerializer.DeserializeAsync<List<DanData>>(danDataFile);
        var gaidenData = await JsonSerializer.DeserializeAsync<List<DanData>>(gaidenDataFile);
        var introData = await JsonSerializer.DeserializeAsync<List<SongIntroductionData>>(songIntroDataFile);
        var movieData = await JsonSerializer.DeserializeAsync<List<MovieData>>(movieDataFile);
        var eventFolderData = await JsonSerializer.DeserializeAsync<List<EventFolderData>>(eventFolderDataFile);
        var shopFolderData = await JsonSerializer.DeserializeAsync<List<ShopFolderData>>(shopFolderDataFile);
        var tokenData = await JsonSerializer.DeserializeAsync<Dictionary<string, int>>(tokenDataFile);
        var lockedSongsData = await JsonSerializer.DeserializeAsync<Dictionary<string, uint[]>>(lockedSongsDataFile);
        var donCosRewardData = await JsonSerializer.DeserializeAsync<DonCosRewards>(donCosRewardFile);
        var shougouData = await JsonSerializer.DeserializeAsync<Shougous>(shougouFile);
        var neiroData = await JsonSerializer.DeserializeAsync<Neiros>(neiroFile);
        var qrCodeData = await JsonSerializer.DeserializeAsync<List<QRCodeData>>(qrCodeDataFile);

        InitializeMusicInfos(infosData);

        InitializeDanData(danData);

        InitializeGaidenData(gaidenData);

        InitializeIntroData(introData);

        InitializeMovieData(movieData);

        InitializeEventFolderData(eventFolderData);

        InitializeShopFolderData(shopFolderData);

        InitializeTokenData(tokenData);

        InitializeLockedSongsData(lockedSongsData);

        InitializeCostumeFlagArraySizes(donCosRewardData);

        InitializeTitleFlagArraySize(shougouData);

        InitializeToneFlagArraySize(neiroData);

        InitializeQrCodeData(qrCodeData);
    }

    private static void DecryptDataTable(string inputFileName, string outputFileName)
    {
        var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.KeySize = 256;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = Convert.FromHexString("3530304242323633353537423431384139353134383346433246464231354534");
        var iv = new byte[16];
        using var fs = File.OpenRead(inputFileName);
        var count = fs.Read(iv, 0, 16);
        count.Throw("Read IV for datatable failed!").IfNotEquals(16);
        aes.IV = iv;
        using var cs = new CryptoStream(fs, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using var ms = new MemoryStream();
        cs.CopyTo(ms);
        ms.Position = 0;
        using var gz = new GZipStream(ms, CompressionMode.Decompress);
        using var output = File.Create(outputFileName);
        gz.CopyTo(output);
    }

    private void InitializeIntroData(List<SongIntroductionData>? introData)
    {
        introData.ThrowIfNull("Shouldn't happen!");
        songIntroductionDictionary = introData.ToImmutableDictionary(data => data.SetId);
    }

    private void InitializeMovieData(List<MovieData>? movieData)
    {
        movieData.ThrowIfNull("Shouldn't happen!");
        movieDataDictionary = movieData.ToImmutableDictionary(data => data.MovieId);
    }

    private void InitializeDanData(List<DanData>? danData)
    {
        danData.ThrowIfNull("Shouldn't happen!");
        commonDanDataDictionary = danData.ToImmutableDictionary(data => data.DanId);
    }

    private void InitializeGaidenData(List<DanData>? gaidenData)
    {
        gaidenData.ThrowIfNull("Shouldn't happen!");
        commonGaidenDataDictionary = gaidenData.ToImmutableDictionary(data => data.DanId);
    }

    private void InitializeEventFolderData(List<EventFolderData>? eventFolderData)
    {
        eventFolderData.ThrowIfNull("Shouldn't happen!");
        eventFolderDictionary = eventFolderData.ToImmutableDictionary(d => d.FolderId);
    }

    private void InitializeMusicInfos(MusicInfos? infosData)
    {
        infosData.ThrowIfNull("Shouldn't happen!");

        musicInfoes = infosData.MusicInfoEntries.ToImmutableDictionary(info => info.MusicId);

        musics = musicInfoes.Select(pair => pair.Key)
            .ToList();
        musics.Sort();

        musicsWithUra = musicInfoes.Where(info => info.Value.StarUra > 0)
            .Select(pair => pair.Key)
            .ToList();
        musicsWithUra.Sort();
    }

    private void InitializeShopFolderData(List<ShopFolderData>? shopFolderData)
    {
        shopFolderData.ThrowIfNull("Shouldn't happen!");
        shopFolderList = shopFolderData;
    }

    private void InitializeTokenData(Dictionary<string, int>? tokenData)
    {
        tokenData.ThrowIfNull("Shouldn't happen!");
        tokenDataDictionary = tokenData;
    }

    private void InitializeLockedSongsData(Dictionary<string, uint[]>? lockedSongsData)
    {
        lockedSongsData.ThrowIfNull("Shouldn't happen!");
        lockedSongsList = lockedSongsData["songNo"].ToList();
    }

    private void InitializeCostumeFlagArraySizes(DonCosRewards? donCosRewardData)
    {
        donCosRewardData.ThrowIfNull("Shouldn't happen!");
        var kigurumiUniqueIdList = donCosRewardData.DonCosRewardEntries
            .Where(entry => entry.CosType == "kigurumi")
            .Select(entry => entry.UniqueId);
        var headUniqueIdList = donCosRewardData.DonCosRewardEntries
            .Where(entry => entry.CosType == "head")
            .Select(entry => entry.UniqueId);
        var bodyUniqueIdList = donCosRewardData.DonCosRewardEntries
            .Where(entry => entry.CosType == "body")
            .Select(entry => entry.UniqueId);
        var faceUniqueIdList = donCosRewardData.DonCosRewardEntries
            .Where(entry => entry.CosType == "face")
            .Select(entry => entry.UniqueId);
        var puchiUniqueIdList = donCosRewardData.DonCosRewardEntries
            .Where(entry => entry.CosType == "puchi")
            .Select(entry => entry.UniqueId);

        costumeFlagArraySizes = new List<int>
        {
            (int)kigurumiUniqueIdList.Max() + 1,
            (int)headUniqueIdList.Max()     + 1,
            (int)bodyUniqueIdList.Max()     + 1,
            (int)faceUniqueIdList.Max()     + 1,
            (int)puchiUniqueIdList.Max()    + 1
        };
    }

    private void InitializeTitleFlagArraySize(Shougous? shougouData)
    {
        shougouData.ThrowIfNull("Shouldn't happen!");
        titleFlagArraySize = (int)shougouData.ShougouEntries.Max(entry => entry.UniqueId) + 1;
    }

    private void InitializeToneFlagArraySize(Neiros? neiroData)
    {
        neiroData.ThrowIfNull("Shouldn't happen!");
        toneFlagArraySize = (int)neiroData.NeiroEntries.Max(entry => entry.UniqueId) + 1;
    }

    private void InitializeQrCodeData(List<QRCodeData>? qrCodeData)
    {
        qrCodeData.ThrowIfNull("Shouldn't happen!");
        qrCodeDataDictionary = qrCodeData.ToImmutableDictionary(data => data.Serial, data => data.Id);
    }
}