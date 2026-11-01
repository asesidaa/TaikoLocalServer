using System.Collections.Immutable;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Application.Catalog;
using TaikoLocalServer.Contracts.AdminApi.Responses;
using Throw;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog;

// TODO(PR2-followup): extract per-table loaders into Loaders/*.cs (spec §3 calls for
// 18 standalone loader classes; deferred to keep PR2 scope manageable).
public partial class FileGameDataCatalog : IGameDataCatalog
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

    private ImmutableDictionary<uint, TelopData> telopDataDictionary =
        ImmutableDictionary<uint, TelopData>.Empty;

    private List<ShopFolderData> shopFolderList = [];

    private uint shopFolderVerup = 1;

    private List<uint> musicUniqueIdList = [];

    private List<uint> musicWithUraUniqueIdList = [];

    private List<uint> lockedSongsList = [];

    private List<uint> timeLimitedSongsList = [];

    private List<uint> lockedUraSongsList = [];
    
    private Dictionary<uint, MusicDetail> musicDetailDictionary = new();

    private List<Costume> costumeList = [];
    
    private Dictionary<uint, Title> titleDictionary = new();
    
    private Dictionary<string, List<uint>> lockedCostumeDataDictionary = new();
    
    private Dictionary<string, List<uint>> lockedTitleDataDictionary = new();

    private List<int> costumeFlagArraySize = [];
    
    private int titleFlagArraySize;

    private int toneFlagArraySize;

    private Dictionary<string, int> tokenDataDictionary = new();

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

    public ImmutableDictionary<uint, TelopData> GetTelopDataDictionary()
    {
        return telopDataDictionary;
    }

    // The default course set (cabinets will pick their own set once they have settings).
    public ImmutableDictionary<uint, DanData> GetCommonDanDataDictionary()
    {
        return danSets.GetValueOrDefault(danIndex.DefaultSet) ?? commonDanDataDictionary;
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

    public uint GetShopFolderVerup()
    {
        return shopFolderVerup;
    }

    public Dictionary<string, int> GetTokenDataDictionary()
    {
        return tokenDataDictionary;
    }

    public List<uint> GetLockedSongsList()
    {
        return lockedSongsList;
    }

    public List<uint> GetTimeLimitedSongsList()
    {
        return timeLimitedSongsList;
    }

    public List<uint> GetLockedUraSongsList()
    {
        return lockedUraSongsList;
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

    // ---------- Loading ----------
    // Each file is parsed into an "apply" step first: a file that fails to parse changes nothing, so the server keeps
    // what it had. Saving from the editors runs the same parser before anything is written.

    private readonly SemaphoreSlim reloadLock = new(1, 1);
    private IReadOnlyList<GameDataFileStatus> fileStatus = [];

    private static readonly string[] DatatableNames =
    [
        CatalogConstants.MusicInfoBaseName, CatalogConstants.WordlistBaseName, CatalogConstants.MusicOrderBaseName,
        CatalogConstants.DonCosRewardBaseName, CatalogConstants.ShougouBaseName, CatalogConstants.NeiroBaseName,
    ];

    /// <summary>The server-owned JSON files: name → parse (throws on bad data) returning the step that applies it.</summary>
    private Dictionary<string, Func<string, Action>> DataParsers => new()
    {
        [DataFiles.Dan] = json => { var d = Parse<List<DanData>>(json).ToImmutableDictionary(x => x.DanId); return () => commonDanDataDictionary = d; },
        [DataFiles.Gaiden] = json => { var d = Parse<List<DanData>>(json).ToImmutableDictionary(x => x.DanId); return () => commonGaidenDataDictionary = d; },
        [DataFiles.Intro] = json => { var d = Parse<List<SongIntroductionData>>(json).ToImmutableDictionary(x => x.SetId); return () => songIntroductionDictionary = d; },
        [DataFiles.Movie] = json => { var d = Parse<List<MovieData>>(json).ToImmutableDictionary(x => x.MovieId); return () => movieDataDictionary = d; },
        [DataFiles.EventFolder] = json => { var d = Parse<List<EventFolderData>>(json).ToImmutableDictionary(x => x.FolderId); return () => eventFolderDictionary = d; },
        [DataFiles.Telop] = json => { var d = Parse<List<TelopData>>(json).ToImmutableDictionary(x => x.TelopId); return () => telopDataDictionary = d; },
        [DataFiles.ShopFolder] = json =>
        {
            var d = Parse<List<ShopFolderData>>(json);
            var verup = (uint)((File.GetLastWriteTime(Path.Combine(PathHelper.GetDataPath(), DataFiles.ShopFolder)).ToFileTime() / 1000) % (1L << 32));
            return () => { shopFolderList = d; shopFolderVerup = verup; };
        },
        [DataFiles.Token] = json => { var d = Parse<Dictionary<string, int>>(json); return () => tokenDataDictionary = d; },
        [DataFiles.LockedSongs] = json =>
        {
            var d = Parse<Dictionary<string, uint[]>>(json);
            var songs = Required(d, "songNo").ToList();
            var ura = Required(d, "uraSongNo").ToList();
            return () => { lockedSongsList = songs; lockedUraSongsList = ura; };
        },
        [DataFiles.SpecialSongs] = json => { var d = Required(Parse<Dictionary<string, uint[]>>(json), "timeLimitedSongNo").ToList(); return () => timeLimitedSongsList = d; },
        [DataFiles.QrCode] = json =>
        {
            var list = Parse<List<QRCodeData>>(json);
            if (list.Any(x => string.IsNullOrWhiteSpace(x.Serial))) throw new InvalidDataException("A QR code has no serial.");
            var dupe = list.GroupBy(x => x.Serial).FirstOrDefault(g => g.Count() > 1);
            if (dupe is not null) throw new InvalidDataException($"Serial \"{dupe.Key}\" is listed twice.");
            var d = list.ToImmutableDictionary(x => x.Serial, x => x.Id);
            return () => qrCodeDataDictionary = d;
        },
        [DataFiles.LockedCostume] = json => { var d = Parse<Dictionary<string, uint[]>>(json).ToDictionary(p => p.Key, p => p.Value.ToList()); return () => lockedCostumeDataDictionary = d; },
        [DataFiles.LockedTitle] = json => { var d = Parse<Dictionary<string, uint[]>>(json).ToDictionary(p => p.Key, p => p.Value.ToList()); return () => lockedTitleDataDictionary = d; },
    };

    private static T Parse<T>(string json) where T : class =>
        JsonSerializer.Deserialize<T>(json) ?? throw new InvalidDataException("The file is empty (null).");

    private static uint[] Required(Dictionary<string, uint[]> d, string key) =>
        d.TryGetValue(key, out var v) ? v : throw new InvalidDataException($"\"{key}\" is missing.");

    public Task InitializeAsync() => ReloadAsync();

    public async Task ReloadAsync()
    {
        await reloadLock.WaitAsync();
        try
        {
            var status = new List<GameDataFileStatus>();
            LoadDatatables(status);
            foreach (var (name, parse) in DataParsers)
            {
                var path = Path.Combine(PathHelper.GetDataPath(), name);
                if (!File.Exists(path))
                {
                    status.Add(new GameDataFileStatus(name, true, true, null, null));
                    continue;
                }
                try
                {
                    parse(await File.ReadAllTextAsync(path))();
                    status.Add(new GameDataFileStatus(name, true, false, null, File.GetLastWriteTimeUtc(path)));
                }
                catch (Exception e) when (e is JsonException or InvalidDataException or IOException or ArgumentException)
                {
                    status.Add(new GameDataFileStatus(name, true, false, e.Message, File.GetLastWriteTimeUtc(path)));
                }
            }
            await LoadDanSetsAsync(status);
            fileStatus = status;
        }
        finally
        {
            reloadLock.Release();
        }
    }

    public IReadOnlyList<GameDataFileStatus> GetFileStatus() => fileStatus;

    public async Task<string?> ReadDataFileAsync(string name)
    {
        // Only the known names: this is also what keeps paths out of the request.
        if (ParserFor(name) is null) return null;
        var path = Path.Combine(PathHelper.GetDataPath(), name);
        return File.Exists(path) ? await File.ReadAllTextAsync(path) : null;
    }

    public async Task<string?> SaveDataFileAsync(string name, string json)
    {
        if (ParserFor(name) is not { } parse) return "Unknown data file.";
        string formatted;
        try
        {
            parse(json);
            formatted = System.Text.Json.Nodes.JsonNode.Parse(json)!.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception e) when (e is JsonException or InvalidDataException or ArgumentException)
        {
            return e.Message;
        }

        await WriteAtomicAsync(Path.Combine(PathHelper.GetDataPath(), name), formatted);
        await ReloadAsync();
        return null;
    }

    // Temp file + rename, so a crash mid-write never leaves half a file; the previous one is kept as .bak.
    private static async Task WriteAtomicAsync(string path, string text)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temp = path + ".tmp";
        await File.WriteAllTextAsync(temp, text);
        if (File.Exists(path)) File.Copy(path, path + ".bak", overwrite: true);
        File.Move(temp, path, overwrite: true);
    }

    private Func<string, Action>? ParserFor(string name) =>
        DataParsers.TryGetValue(name, out var parse) ? parse
        : DanSetId(name) is { } id && danIndex.Sets.Any(x => x.Id == id) ? DanSetParser(id)
        : null;

    // The game's datatables depend on each other (names come from the wordlist), so they load as one group.
    private void LoadDatatables(List<GameDataFileStatus> status)
    {
        var dir = PathHelper.GetDatatablePath();
        foreach (var name in DatatableNames)
        {
            var bin = Path.Combine(dir, $"{name}.bin");
            if (File.Exists(bin)) DecryptDataTable(bin, Path.Combine(dir, $"{name}.json"));
        }

        var missing = DatatableNames.Where(n => !File.Exists(Path.Combine(dir, $"{n}.json"))).ToHashSet();
        string? error = null;
        if (missing.Count == 0)
        {
            try
            {
                T Read<T>(string n) where T : class => Parse<T>(File.ReadAllText(Path.Combine(dir, $"{n}.json")));
                var musicInfo = Read<MusicInfos>(CatalogConstants.MusicInfoBaseName);
                var wordlist = Read<WordList>(CatalogConstants.WordlistBaseName);
                var musicOrder = Read<MusicOrder>(CatalogConstants.MusicOrderBaseName);
                var donCos = Read<DonCosRewards>(CatalogConstants.DonCosRewardBaseName);
                var shougou = Read<Shougous>(CatalogConstants.ShougouBaseName);
                var neiro = Read<Neiros>(CatalogConstants.NeiroBaseName);

                InitializeMusicInfos(musicInfo);
                InitializeMusicDetails(musicInfo, musicOrder, wordlist);
                InitializeCostumes(donCos, wordlist);
                InitializeTitles(shougou, wordlist);
                InitializeToneFlagArraySize(neiro);
            }
            catch (Exception e) when (e is JsonException or InvalidDataException or IOException or InvalidOperationException or CryptographicException)
            {
                error = e.Message;
            }
        }
        foreach (var name in DatatableNames)
        {
            var path = Path.Combine(dir, $"{name}.bin");
            status.Add(new GameDataFileStatus($"datatable/{name}.bin", false, missing.Contains(name), error,
                File.Exists(path) ? File.GetLastWriteTimeUtc(path) : null));
        }
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

    private void InitializeMusicInfos(MusicInfos infosData)
    {
        musicInfos = infosData.MusicInfoEntries.ToImmutableDictionary(info => info.MusicId);

        musicUniqueIdList = musicInfos.Select(pair => pair.Key)
            .ToList();
        musicUniqueIdList.Sort();

        musicWithUraUniqueIdList = musicInfos.Where(info => info.Value.StarUra > 0)
            .Select(pair => pair.Key)
            .ToList();
        musicWithUraUniqueIdList.Sort();
    }

    private void InitializeMusicDetails(MusicInfos musicInfoData, MusicOrder musicOrderData, WordList wordlistData)
    {
        var details = new Dictionary<uint, MusicDetail>();
        // One pass over the wordlist instead of a search per song.
        var words = wordlistData.WordListEntries.GroupBy(e => e.Key).ToDictionary(g => g.Key, g => g.First());
        foreach (var musicInfo in musicInfoData.MusicInfoEntries)
        {
            var musicUniqueId = musicInfo.MusicId;
            if (musicUniqueId == 0)
            {
                continue;
            }

            // Missing song_sub entries are common, so names default to empty rather than failing.
            var name = words.GetValueOrDefault($"song_{musicInfo.Id}");
            var artist = words.GetValueOrDefault($"song_sub_{musicInfo.Id}");
            details.TryAdd(musicUniqueId, new MusicDetail
            {
                SongId = musicUniqueId,
                SongName = name?.JapaneseText ?? "",
                SongNameEN = name?.EnglishUsText ?? "",
                SongNameCN = name?.ChineseTText ?? "",
                SongNameKO = name?.KoreanText ?? "",
                ArtistName = artist?.JapaneseText ?? "",
                ArtistNameEN = artist?.EnglishUsText ?? "",
                ArtistNameCN = artist?.ChineseTText ?? "",
                ArtistNameKO = artist?.KoreanText ?? "",
                Genre = musicInfo.Genre,
                StarEasy = musicInfo.StarEasy,
                StarNormal = musicInfo.StarNormal,
                StarHard = musicInfo.StarHard,
                StarOni = musicInfo.StarOni,
                StarUra = musicInfo.StarUra
            });
        }

        for (var index = 0; index < musicOrderData.Order.Count; index++)
        {
            if (details.TryGetValue(musicOrderData.Order[index].SongId, out var musicDetail))
            {
                musicDetail.Index = index;
            }
        }
        musicDetailDictionary = details;
    }

    private void InitializeCostumes(DonCosRewards donCosRewardData, WordList wordlistData)
    {
        var words = wordlistData.WordListEntries.GroupBy(e => e.Key).ToDictionary(g => g.Key, g => g.First());
        var costumes = new List<Costume>();
        foreach (var donCosReward in donCosRewardData.DonCosRewardEntries)
        {
            var word = words.TryGetValue($"costume_{donCosReward.CosType}_{donCosReward.UniqueId}", out var w)
                ? w : throw new InvalidDataException($"The wordlist has no name for costume {donCosReward.CosType} {donCosReward.UniqueId}.");
            costumes.Add(new Costume
            {
                CostumeId = donCosReward.UniqueId,
                CostumeType = donCosReward.CosType,
                CostumeName = word.JapaneseText,
                CostumeNameEN = word.EnglishUsText,
                CostumeNameCN = word.ChineseTText,
                CostumeNameKO = word.KoreanText,
            });
        }

        int Size(string type) => (int)costumes.Where(c => c.CostumeType == type).Max(c => c.CostumeId) + 1;
        costumeFlagArraySize = [Size("kigurumi"), Size("head"), Size("body"), Size("face"), Size("puchi")];
        costumeList = costumes;
    }

    private void InitializeTitles(Shougous shougouData, WordList wordlistData)
    {
        var words = wordlistData.WordListEntries.GroupBy(e => e.Key).ToDictionary(g => g.Key, g => g.First());
        var titles = new Dictionary<uint, Title>();
        foreach (var shougou in shougouData.ShougouEntries)
        {
            var word = words.TryGetValue($"syougou_{shougou.UniqueId}", out var w)
                ? w : throw new InvalidDataException($"The wordlist has no name for title {shougou.UniqueId}.");
            titles.TryAdd(shougou.UniqueId, new Title
            {
                TitleId = shougou.UniqueId,
                TitleName = word.JapaneseText,
                TitleNameEN = word.EnglishUsText,
                TitleNameCN = word.ChineseTText,
                TitleNameKO = word.KoreanText,
                TitleRarity = shougou.Rarity
            });
        }

        titleFlagArraySize = (int)titles.Max(title => title.Key) + 1;
        titleDictionary = titles;
    }

    private void InitializeToneFlagArraySize(Neiros neiroData)
    {
        toneFlagArraySize = (int)neiroData.NeiroEntries.Max(entry => entry.UniqueId) + 1;
    }
}
