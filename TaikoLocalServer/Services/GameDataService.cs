using ICSharpCode.SharpZipLib.GZip;
using Microsoft.Extensions.Options;
using SharedProject.Models;
using SharedProject.Utils;
using Swan.Mapping;
using System.Collections.Immutable;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using TaikoLocalServer.Settings;
using Throw;

namespace TaikoLocalServer.Services;

public class GameDataService : IGameDataService
{
	private ImmutableDictionary<uint, MusicInfoEntry> musicInfoes =
		ImmutableDictionary<uint, MusicInfoEntry>.Empty;

	private ImmutableDictionary<uint, GetDanOdaiResponse.OdaiData> danDataDictionary =
		ImmutableDictionary<uint, GetDanOdaiResponse.OdaiData>.Empty;

	private ImmutableDictionary<uint, GetDanOdaiResponse.OdaiData> gaidenDataDictionary =
		ImmutableDictionary<uint, GetDanOdaiResponse.OdaiData>.Empty;

	private ImmutableDictionary<uint, GetSongIntroductionResponse.SongIntroductionData> introDataDictionary =
		ImmutableDictionary<uint, GetSongIntroductionResponse.SongIntroductionData>.Empty;

	private ImmutableDictionary<uint, InitialdatacheckResponse.MovieData> movieDataDictionary =
		ImmutableDictionary<uint, InitialdatacheckResponse.MovieData>.Empty;

	private ImmutableDictionary<uint, GetfolderResponse.EventfolderData> folderDictionary =
		ImmutableDictionary<uint, GetfolderResponse.EventfolderData>.Empty;

	private ImmutableDictionary<string, uint> qrCodeDataDictionary = ImmutableDictionary<string, uint>.Empty;
	
	private List<GetShopFolderResponse.ShopFolderData> shopFolderList = new();

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

	public ImmutableDictionary<uint, MusicInfoEntry> GetMusicInfoes()
	{
		return musicInfoes;
	}

	public ImmutableDictionary<uint, GetDanOdaiResponse.OdaiData> GetDanDataDictionary()
	{
		return danDataDictionary;
	}

	public ImmutableDictionary<uint, GetDanOdaiResponse.OdaiData> GetGaidenDataDictionary()
	{
		return gaidenDataDictionary;
	}

	public ImmutableDictionary<uint, GetSongIntroductionResponse.SongIntroductionData> GetSongIntroDictionary()
	{
		return introDataDictionary;
	}

	public ImmutableDictionary<uint, InitialdatacheckResponse.MovieData> GetMovieDataDictionary()
	{
		return movieDataDictionary;
	}

	public ImmutableDictionary<uint, GetfolderResponse.EventfolderData> GetFolderDictionary()
	{
		return folderDictionary;
	}

	public List<GetShopFolderResponse.ShopFolderData> GetShopFolderList()
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
		
		var musicInfoPath = Path.Combine(dataPath, $"{Constants.MUSIC_INFO_BASE_NAME}.json");
		var encryptedInfo = Path.Combine(dataPath, $"{Constants.MUSIC_INFO_BASE_NAME}.bin");
		
		var wordlistPath = Path.Combine(dataPath, $"{Constants.WORDLIST_BASE_NAME}.json");
		var encryptedWordlist = Path.Combine(dataPath, $"{Constants.WORDLIST_BASE_NAME}.bin");
		
		var musicOrderPath = Path.Combine(dataPath, $"{Constants.MUSIC_ORDER_BASE_NAME}.json");
		var encryptedMusicOrder = Path.Combine(dataPath, $"{Constants.MUSIC_ORDER_BASE_NAME}.bin");
		
		var donCosRewardPath = Path.Combine(dataPath, $"{Constants.DON_COS_REWARD_BASE_NAME}.json");
		var encryptedDonCosReward = Path.Combine(dataPath, $"{Constants.DON_COS_REWARD_BASE_NAME}.bin");
		
		var shougouPath = Path.Combine(dataPath, $"{Constants.SHOUGOU_BASE_NAME}.json");
		var encryptedShougou = Path.Combine(dataPath, $"{Constants.SHOUGOU_BASE_NAME}.bin");
		
		var neiroPath = Path.Combine(dataPath, $"{Constants.NEIRO_BASE_NAME}.json");
		var encryptedNeiro = Path.Combine(dataPath, $"{Constants.NEIRO_BASE_NAME}.bin");
		
		var danDataPath = Path.Combine(dataPath, settings.DanDataFileName);
		var gaidenDataPath = Path.Combine(dataPath, settings.GaidenDataFileName);
		var songIntroDataPath = Path.Combine(dataPath, settings.IntroDataFileName);
		var movieDataPath = Path.Combine(dataPath, settings.MovieDataFileName);
		var eventFolderDataPath = Path.Combine(dataPath, settings.EventFolderDataFileName);
		var shopFolderDataPath = Path.Combine(dataPath, settings.ShopFolderDataFileName);
		var tokenDataPath = Path.Combine(dataPath, settings.TokenDataFileName);
		var lockedSongsDataPath = Path.Combine(dataPath, settings.LockedSongsDataFileName);
		var qrCodeDataPath = Path.Combine(dataPath, settings.QRCodeDataFileName);

		if (File.Exists(encryptedInfo))
		{
			DecryptDataTable(encryptedInfo, musicInfoPath);
		}
		if (File.Exists(encryptedWordlist))
		{
			DecryptDataTable(encryptedWordlist, wordlistPath);
		}
		if (File.Exists(encryptedMusicOrder))
		{
			DecryptDataTable(encryptedMusicOrder, musicOrderPath);
		}
		if (File.Exists(encryptedDonCosReward))
		{
			DecryptDataTable(encryptedDonCosReward, donCosRewardPath);
		}
		if (File.Exists(encryptedShougou))
		{
			DecryptDataTable(encryptedShougou, shougouPath);
		}
		if (File.Exists(encryptedNeiro))
		{
			DecryptDataTable(encryptedNeiro, neiroPath);
		}
		
		if (!File.Exists(musicInfoPath))
		{
			throw new FileNotFoundException("Music info file not found!");
		}
		if (!File.Exists(wordlistPath))
		{
			throw new FileNotFoundException("Wordlist file not found!");
		}
		if (!File.Exists(musicOrderPath))
		{
			throw new FileNotFoundException("Music order file not found!");
		}
		if (!File.Exists(donCosRewardPath))
		{
			throw new FileNotFoundException("Don cos reward file not found!");
		}
		if (!File.Exists(shougouPath))
		{
			throw new FileNotFoundException("Shougou file not found!");
		}
		if (!File.Exists(neiroPath))
		{
			throw new FileNotFoundException("Neiro file not found!");
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

		var infoesData = await JsonSerializer.DeserializeAsync<MusicInfoes>(musicInfoFile);
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

		InitializeMusicInfoes(infoesData);

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
		introDataDictionary = introData.ToImmutableDictionary(data => data.SetId, ToResponseIntroData);
	}

	private void InitializeMovieData(List<MovieData>? movieData)
	{
		movieData.ThrowIfNull("Shouldn't happen!");
		movieDataDictionary = movieData.ToImmutableDictionary(data => data.MovieId, ToResponseMovieData);
	}

	private void InitializeDanData(List<DanData>? danData)
	{
		danData.ThrowIfNull("Shouldn't happen!");
		danDataDictionary = danData.ToImmutableDictionary(data => data.DanId, ToResponseOdaiData);
	}

	private void InitializeGaidenData(List<DanData>? gaidenData)
	{
		gaidenData.ThrowIfNull("Shouldn't happen!");
		gaidenDataDictionary = gaidenData.ToImmutableDictionary(data => data.DanId, ToResponseOdaiData);
	}

	private void InitializeEventFolderData(List<EventFolderData>? eventFolderData)
	{
		eventFolderData.ThrowIfNull("Shouldn't happen!");
		folderDictionary = eventFolderData.ToImmutableDictionary(data => data.FolderId, ToResponseEventFolderData);
	}

	private void InitializeMusicInfoes(MusicInfoes? infoesData)
	{
		infoesData.ThrowIfNull("Shouldn't happen!");

		musicInfoes = infoesData.MusicInfoEntries.ToImmutableDictionary(info => info.MusicId);

		musics = musicInfoes.Select(pair => pair.Key)
			.ToList();
		musics.Sort();
		
		musicsWithUra = musicInfoes.Where(info => info.Value.starUra > 0)
			.Select(pair => pair.Key)
			.ToList();
		musicsWithUra.Sort();
	}

	private void InitializeShopFolderData(List<ShopFolderData>? shopFolderData)
	{
		shopFolderData.ThrowIfNull("Shouldn't happen!");
		foreach (var folderData in shopFolderData)
		{
			shopFolderList.Add(ToResponseShopFolderData(folderData));
		}
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
			.Where(entry => entry.cosType == "kigurumi")
			.Select(entry => entry.uniqueId);
		var headUniqueIdList = donCosRewardData.DonCosRewardEntries
			.Where(entry => entry.cosType == "head")
			.Select(entry => entry.uniqueId);
		var bodyUniqueIdList = donCosRewardData.DonCosRewardEntries
			.Where(entry => entry.cosType == "body")
			.Select(entry => entry.uniqueId);
		var faceUniqueIdList = donCosRewardData.DonCosRewardEntries
			.Where(entry => entry.cosType == "face")
			.Select(entry => entry.uniqueId);
		var puchiUniqueIdList = donCosRewardData.DonCosRewardEntries
			.Where(entry => entry.cosType == "puchi")
			.Select(entry => entry.uniqueId);
		
		costumeFlagArraySizes = new List<int>
		{
			(int)kigurumiUniqueIdList.Max(),
			(int)headUniqueIdList.Max(),
			(int)bodyUniqueIdList.Max(),
			(int)faceUniqueIdList.Max(),
			(int)puchiUniqueIdList.Max()
		};
	}
	
	private void InitializeTitleFlagArraySize(Shougous? shougouData)
	{
		shougouData.ThrowIfNull("Shouldn't happen!");
		titleFlagArraySize = (int)shougouData.ShougouEntries.Max(entry => entry.uniqueId);
	}
	
	private void InitializeToneFlagArraySize(Neiros? neiroData)
	{
		neiroData.ThrowIfNull("Shouldn't happen!");
		toneFlagArraySize = (int)neiroData.NeiroEntries.Max(entry => entry.uniqueId);
	}
	
	private void InitializeQrCodeData(List<QRCodeData>? qrCodeData)
	{
		qrCodeData.ThrowIfNull("Shouldn't happen!");
		qrCodeDataDictionary = qrCodeData.ToImmutableDictionary(data => data.Serial, data => data.Id);
	}

	private static GetDanOdaiResponse.OdaiData ToResponseOdaiData(DanData data)
	{
		var responseOdaiData = new GetDanOdaiResponse.OdaiData
		{
			DanId = data.DanId,
			Title = data.Title,
			VerupNo = data.VerupNo
		};

		var odaiSongs = data.OdaiSongList.Select(song => song.CopyPropertiesToNew<GetDanOdaiResponse.OdaiData.OdaiSong>());
		responseOdaiData.AryOdaiSongs.AddRange(odaiSongs);

		var odaiBorders = data.OdaiBorderList.Select(border => border.CopyPropertiesToNew<GetDanOdaiResponse.OdaiData.OdaiBorder>());
		responseOdaiData.AryOdaiBorders.AddRange(odaiBorders);

		return responseOdaiData;
	}

	private static GetSongIntroductionResponse.SongIntroductionData ToResponseIntroData(SongIntroductionData data)
	{
		var responseIntroData = new GetSongIntroductionResponse.SongIntroductionData
		{
			SetId = data.SetId,
			VerupNo = data.VerupNo,
			MainSongNo = data.MainSongNo,
			SubSongNoes = data.SubSongNo
		};

		return responseIntroData;
	}

	private static InitialdatacheckResponse.MovieData ToResponseMovieData(MovieData data)
	{
		var responseMovieData = new InitialdatacheckResponse.MovieData
		{
			MovieId = data.MovieId,
			EnableDays = data.EnableDays
		};

		return responseMovieData;
	}

	private static GetfolderResponse.EventfolderData ToResponseEventFolderData(EventFolderData data)
	{
		var responseEventFolderData = new GetfolderResponse.EventfolderData
		{
			FolderId = data.FolderId,
			VerupNo = data.VerupNo,
			Priority = data.Priority,
			SongNoes = data.SongNo,
			ParentFolderId = data.ParentFolderId
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