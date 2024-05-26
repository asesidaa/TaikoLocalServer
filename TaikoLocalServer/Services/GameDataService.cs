using Microsoft.Extensions.Options;
using SharedProject.Models;
using SharedProject.Utils;
using System.Collections.Immutable;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using TaikoLocalServer.Settings;
using TaikoWebUI.Shared.Models;
using Throw;

namespace TaikoLocalServer.Services;

public class GameDataService(IOptions<DataSettings> dataSettings) : IGameDataService
{
    private ImmutableDictionary<uint, DanData> commonDanDataDictionary =
        ImmutableDictionary<uint, DanData>.Empty;

    private ImmutableDictionary<uint, DanData> commonGaidenDataDictionary =
        ImmutableDictionary<uint, DanData>.Empty;

    private ImmutableDictionary<uint, MusicInfoEntry> musicInfos =
        ImmutableDictionary<uint, MusicInfoEntry>.Empty;
    
    private ImmutableDictionary<uint, MovieData> movieDataDictionary =
        ImmutableDictionary<uint, MovieData>.Empty;

    private ImmutableDictionary<uint, SongIntroductionData> songIntroductionDictionary =
        ImmutableDictionary<uint, SongIntroductionData>.Empty;
    
    private ImmutableDictionary<string, uint> qrCodeDataDictionary = ImmutableDictionary<string, uint>.Empty;
    
    private ImmutableDictionary<uint, EventFolderData> eventFolderDictionary = 
        ImmutableDictionary<uint, EventFolderData>.Empty;

    private List<ShopFolderData> shopFolderList = [];

    private List<uint> musicUniqueIdList = [];

    private List<uint> musicWithUraUniqueIdList = [];

    private List<uint> lockedSongsList = [];
    
    private readonly Dictionary<uint, MusicDetail> musicDetailDictionary = new();

    private readonly List<Costume> costumeList = [];
    
    private readonly Dictionary<uint, Title> titleDictionary = new();
    
    private Dictionary<string, List<uint>> lockedCostumeDataDictionary = new();
    
    private Dictionary<string, List<uint>> lockedTitleDataDictionary = new();

    private List<int> costumeFlagArraySize = [];
    
    private int titleFlagArraySize;

    private int toneFlagArraySize;

    private Dictionary<string, int> tokenDataDictionary = new();

    private readonly DataSettings settings = dataSettings.Value;

    public List<uint> GetMusicList()
    {
        return musicUniqueIdList;
    }

    public List<uint> GetMusicWithUraList()
    {
        return musicWithUraUniqueIdList;
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
    
    public Dictionary<uint, MusicDetail> GetMusicDetailDictionary()
    {
        return musicDetailDictionary;
    }
    
    public List<Costume> GetCostumeList()
    {
        return costumeList;
    }
    
    public Dictionary<uint, Title> GetTitleDictionary()
    {
        return titleDictionary;
    }
    
    public Dictionary<string, List<uint>> GetLockedCostumeDataDictionary()
    {
        return lockedCostumeDataDictionary;
    }
    
    public Dictionary<string, List<uint>> GetLockedTitleDataDictionary()
    {
        return lockedTitleDataDictionary;
    }

    public List<int> GetCostumeFlagArraySizes()
    {
        return costumeFlagArraySize;
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

        var musicInfoPath = Path.Combine(datatablePath, $"{Constants.MusicInfoBaseName}.json");
        var encryptedInfo = Path.Combine(datatablePath, $"{Constants.MusicInfoBaseName}.bin");

        var wordlistPath = Path.Combine(datatablePath, $"{Constants.WordlistBaseName}.json");
        var encryptedWordlist = Path.Combine(datatablePath, $"{Constants.WordlistBaseName}.bin");

        var musicOrderPath = Path.Combine(datatablePath, $"{Constants.MusicOrderBaseName}.json");
        var encryptedMusicOrder = Path.Combine(datatablePath, $"{Constants.MusicOrderBaseName}.bin");

        var donCosRewardPath = Path.Combine(datatablePath, $"{Constants.DonCosRewardBaseName}.json");
        var encryptedDonCosReward = Path.Combine(datatablePath, $"{Constants.DonCosRewardBaseName}.bin");

        var shougouPath = Path.Combine(datatablePath, $"{Constants.ShougouBaseName}.json");
        var encryptedShougou = Path.Combine(datatablePath, $"{Constants.ShougouBaseName}.bin");

        var neiroPath = Path.Combine(datatablePath, $"{Constants.NeiroBaseName}.json");
        var encryptedNeiro = Path.Combine(datatablePath, $"{Constants.NeiroBaseName}.bin");

        var danDataPath = Path.Combine(dataPath, settings.DanDataFileName);
        var gaidenDataPath = Path.Combine(dataPath, settings.GaidenDataFileName);
        var songIntroDataPath = Path.Combine(dataPath, settings.IntroDataFileName);
        var movieDataPath = Path.Combine(dataPath, settings.MovieDataFileName);
        var eventFolderDataPath = Path.Combine(dataPath, settings.EventFolderDataFileName);
        var shopFolderDataPath = Path.Combine(dataPath, settings.ShopFolderDataFileName);
        var tokenDataPath = Path.Combine(dataPath, settings.TokenDataFileName);
        var lockedSongsDataPath = Path.Combine(dataPath, settings.LockedSongsDataFileName);
        var qrCodeDataPath = Path.Combine(dataPath, settings.QrCodeDataFileName);
        var lockedCostumeDataPath = Path.Combine(dataPath, settings.LockedCostumeDataFileName);
        var lockedTitleDataPath = Path.Combine(dataPath, settings.LockedTitleDataFileName);

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

        foreach (var filePath in outputPaths.Where(filePath => !File.Exists(filePath)))
        {
            throw new FileNotFoundException($"{Path.GetFileName(filePath)} file not found!");
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
        await using var wordlistFile = File.OpenRead(wordlistPath);
        await using var musicOrderFile = File.OpenRead(musicOrderPath);
        await using var lockedCostumeDataFile = File.OpenRead(lockedCostumeDataPath);
        await using var lockedTitleDataFile = File.OpenRead(lockedTitleDataPath);

        var musicInfoData = await JsonSerializer.DeserializeAsync<MusicInfos>(musicInfoFile);
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
        var wordlistData = await JsonSerializer.DeserializeAsync<WordList>(wordlistFile);
        var musicOrderData = await JsonSerializer.DeserializeAsync<MusicOrder>(musicOrderFile);
        var lockedCostumeData = await JsonSerializer.DeserializeAsync<Dictionary<string, uint[]>>(lockedCostumeDataFile);
        var lockedTitleData = await JsonSerializer.DeserializeAsync<Dictionary<string, uint[]>>(lockedTitleDataFile);

        InitializeMusicInfos(musicInfoData);

        InitializeDanData(danData);

        InitializeGaidenData(gaidenData);

        InitializeIntroData(introData);

        InitializeMovieData(movieData);

        InitializeEventFolderData(eventFolderData);

        InitializeShopFolderData(shopFolderData);

        InitializeTokenData(tokenData);

        InitializeLockedSongsData(lockedSongsData);
        
        InitializeMusicDetails(musicInfoData, musicOrderData, wordlistData);

        InitializeCostumes(donCosRewardData, wordlistData);

        InitializeTitles(shougouData, wordlistData);
        
        InitializeLockedCostumeData(lockedCostumeData);
        
        InitializeLockedTitleData(lockedTitleData);

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

        musicInfos = infosData.MusicInfoEntries.ToImmutableDictionary(info => info.MusicId);

        musicUniqueIdList = musicInfos.Select(pair => pair.Key)
            .ToList();
        musicUniqueIdList.Sort();

        musicWithUraUniqueIdList = musicInfos.Where(info => info.Value.StarUra > 0)
            .Select(pair => pair.Key)
            .ToList();
        musicWithUraUniqueIdList.Sort();
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
    
    private void InitializeMusicDetails(MusicInfos? musicInfoData, MusicOrder? musicOrderData, WordList? wordlistData)
    {
        musicInfoData.ThrowIfNull("Shouldn't happen!");
        musicOrderData.ThrowIfNull("Shouldn't happen!");
        wordlistData.ThrowIfNull("Shouldn't happen!");
        
        foreach (var musicInfo in musicInfoData.MusicInfoEntries)
        {
            var musicId = musicInfo.Id;
            var musicNameKey = $"song_{musicId}";
            var musicArtistKey = $"song_sub_{musicId}";
            var musicName = wordlistData.WordListEntries.First(entry => entry.Key == musicNameKey).JapaneseText;
            var musicArtist = wordlistData.WordListEntries.First(entry => entry.Key == musicArtistKey).JapaneseText;
            var musicNameEn = wordlistData.WordListEntries.First(entry => entry.Key == musicNameKey).EnglishUsText;
            var musicArtistEn = wordlistData.WordListEntries.First(entry => entry.Key == musicArtistKey).EnglishUsText;
            var musicNameCn = wordlistData.WordListEntries.First(entry => entry.Key == musicNameKey).ChineseTText;
            var musicArtistCn = wordlistData.WordListEntries.First(entry => entry.Key == musicArtistKey).ChineseTText;
            var musicNameKo = wordlistData.WordListEntries.First(entry => entry.Key == musicNameKey).KoreanText;
            var musicArtistKo = wordlistData.WordListEntries.First(entry => entry.Key == musicArtistKey).KoreanText;
            var musicUniqueId = musicInfo.MusicId;
            var musicGenre = musicInfo.Genre;
            var musicStarEasy = musicInfo.StarEasy;
            var musicStarNormal = musicInfo.StarNormal;
            var musicStarHard = musicInfo.StarHard;
            var musicStarOni = musicInfo.StarOni;
            var musicStarUra = musicInfo.StarUra;
            var musicDetail = new MusicDetail
            {
                SongId = musicUniqueId,
                SongName = musicName,
                SongNameEN = musicNameEn,
                SongNameCN = musicNameCn,
                SongNameKO = musicNameKo,
                ArtistName = musicArtist,
                ArtistNameEN = musicArtistEn,
                ArtistNameCN = musicArtistCn,
                ArtistNameKO = musicArtistKo,
                Genre = musicGenre,
                StarEasy = musicStarEasy,
                StarNormal = musicStarNormal,
                StarHard = musicStarHard,
                StarOni = musicStarOni,
                StarUra = musicStarUra
            };
            musicDetailDictionary.TryAdd(musicUniqueId, musicDetail);
        }
        
        for (var index = 0; index < musicOrderData.Order.Count; index++)
        {
            var musicOrderEntry = musicOrderData.Order[index];
            var musicUniqueId = musicOrderEntry.SongId;
            if (musicDetailDictionary.TryGetValue(musicUniqueId, out var musicDetail))
            {
                musicDetail.Index = index;
            }
        }
    }

    private void InitializeCostumes(DonCosRewards? donCosRewardData, WordList? wordlistData)
    {
        donCosRewardData.ThrowIfNull("Shouldn't happen!");
        wordlistData.ThrowIfNull("Shouldn't happen!");

        foreach (var donCosReward in donCosRewardData.DonCosRewardEntries)
        {
            var cosType = donCosReward.CosType;
            var costumeId = donCosReward.UniqueId;

            var costumeNameKey = $"costume_{cosType}_{costumeId}";
            var costumeName = wordlistData.WordListEntries.First(entry => entry.Key == costumeNameKey).JapaneseText;
            var costume = new Costume
            {
                CostumeId = costumeId,
                CostumeType = cosType,
                CostumeName = costumeName
            };
            costumeList.Add(costume);
        }

        var kigurumiMaxArraySize = (int)costumeList.Where(costume => costume.CostumeType == "kigurumi").Max(costume => costume.CostumeId) + 1;
        var headMaxArraySize = (int)costumeList.Where(costume => costume.CostumeType == "head").Max(costume => costume.CostumeId) + 1;
        var bodyMaxArraySize = (int)costumeList.Where(costume => costume.CostumeType == "body").Max(costume => costume.CostumeId) + 1;
        var faceMaxArraySize = (int)costumeList.Where(costume => costume.CostumeType == "face").Max(costume => costume.CostumeId) + 1;
        var puchiMaxArraySize = (int)costumeList.Where(costume => costume.CostumeType == "puchi").Max(costume => costume.CostumeId) + 1;
        costumeFlagArraySize =
            [kigurumiMaxArraySize, headMaxArraySize, bodyMaxArraySize, faceMaxArraySize, puchiMaxArraySize];
    }

    private void InitializeTitles(Shougous? shougouData, WordList? wordlistData)
    {
        shougouData.ThrowIfNull("Shouldn't happen!");
        wordlistData.ThrowIfNull("Shouldn't happen!");
    
        foreach (var shougou in shougouData.ShougouEntries)
        {
            var titleId = shougou.UniqueId;
            var titleNameKey = $"syougou_{titleId}";
            var titleName = wordlistData.WordListEntries.First(entry => entry.Key == titleNameKey).JapaneseText;
            var title = new Title
            {
                TitleId = titleId,
                TitleName = titleName,
                TitleRarity = shougou.Rarity
            };
            titleDictionary.TryAdd(titleId, title);
        }
        
        titleFlagArraySize = (int)titleDictionary.Max(title => title.Key) + 1;
    }
    
    private void InitializeLockedCostumeData(Dictionary<string, uint[]>? lockedCostumeData)
    {
        lockedCostumeData.ThrowIfNull("Shouldn't happen!");
        lockedCostumeDataDictionary = lockedCostumeData.ToDictionary(pair => pair.Key, pair => pair.Value.ToList());
    }
    
    private void InitializeLockedTitleData(Dictionary<string, uint[]>? lockedTitleData)
    {
        lockedTitleData.ThrowIfNull("Shouldn't happen!");
        lockedTitleDataDictionary = lockedTitleData.ToDictionary(pair => pair.Key, pair => pair.Value.ToList());
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