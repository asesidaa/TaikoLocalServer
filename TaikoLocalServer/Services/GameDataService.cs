using ICSharpCode.SharpZipLib.GZip;
using Microsoft.Extensions.Options;
using SharedProject.Models;
using SharedProject.Utils;
using Swan.Mapping;
using System.Collections.Immutable;
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

	private ImmutableDictionary<uint, MusicAttributeEntry> musicAttributes =
		ImmutableDictionary<uint, MusicAttributeEntry>.Empty;

	private ImmutableDictionary<uint, GetfolderResponse.EventfolderData> folderDictionary =
		ImmutableDictionary<uint, GetfolderResponse.EventfolderData>.Empty;

	private ImmutableDictionary<uint, GetShopFolderResponse.ShopFolderData> shopFolderDictionary =
		ImmutableDictionary<uint, GetShopFolderResponse.ShopFolderData>.Empty;

	private ImmutableDictionary<string, uint> qrCodeDataDictionary = ImmutableDictionary<string, uint>.Empty;

	private List<uint> musics = new();

	private List<uint> musicsWithUra = new();

	private List<uint> lockedSongsList = new();

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

	public ImmutableDictionary<uint, MusicAttributeEntry> GetMusicAttributes()
	{
		return musicAttributes;
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

	public ImmutableDictionary<uint, GetShopFolderResponse.ShopFolderData> GetShopFolderDictionary()
	{
		return shopFolderDictionary;
	}

	public Dictionary<string, int> GetTokenDataDictionary()
	{
		return tokenDataDictionary;
	}

	public List<uint> GetLockedSongsList()
	{
		return lockedSongsList;
	}

	public ImmutableDictionary<string, uint> GetQRCodeDataDictionary()
	{
		return qrCodeDataDictionary;
	}

	public async Task InitializeAsync()
	{
		var dataPath = PathHelper.GetDataPath();
		var musicInfoPath = Path.Combine(dataPath, Constants.MUSIC_INFO_FILE_NAME);
		var compressedMusicInfoPath = Path.Combine(dataPath, Constants.MUSIC_INFO_COMPRESSED_FILE_NAME);
		var musicAttributePath = Path.Combine(dataPath, Constants.MUSIC_ATTRIBUTE_FILE_NAME);
		var compressedMusicAttributePath = Path.Combine(dataPath, Constants.MUSIC_ATTRIBUTE_COMPRESSED_FILE_NAME);
		var danDataPath = Path.Combine(dataPath, settings.DanDataFileName);
		var gaidenDataPath = Path.Combine(dataPath, settings.GaidenDataFileName);
		var songIntroDataPath = Path.Combine(dataPath, settings.IntroDataFileName);
		var movieDataPath = Path.Combine(dataPath, settings.MovieDataFileName);
		var eventFolderDataPath = Path.Combine(dataPath, settings.EventFolderDataFileName);
		var shopFolderDataPath = Path.Combine(dataPath, settings.ShopFolderDataFileName);
		var tokenDataPath = Path.Combine(dataPath, settings.TokenDataFileName);
		var lockedSongsDataPath = Path.Combine(dataPath, settings.LockedSongsDataFileName);
		var qrCodeDataPath = Path.Combine(dataPath, settings.QRCodeDataFileName);

		if (File.Exists(compressedMusicInfoPath))
		{
			TryDecompressMusicInfo();
		}
		if (File.Exists(compressedMusicAttributePath))
		{
			TryDecompressMusicAttribute();
		}
		await using var musicInfoFile = File.OpenRead(musicInfoPath);
		await using var musicAttributeFile = File.OpenRead(musicAttributePath);
		await using var danDataFile = File.OpenRead(danDataPath);
		await using var gaidenDataFile = File.OpenRead(gaidenDataPath);
		await using var songIntroDataFile = File.OpenRead(songIntroDataPath);
		await using var movieDataFile = File.OpenRead(movieDataPath);
		await using var eventFolderDataFile = File.OpenRead(eventFolderDataPath);
		await using var shopFolderDataFile = File.OpenRead(shopFolderDataPath);
		await using var tokenDataFile = File.OpenRead(tokenDataPath);
		await using var lockedSongsDataFile = File.OpenRead(lockedSongsDataPath);
		await using var qrCodeDataFile = File.OpenRead(qrCodeDataPath);

		var infoesData = await JsonSerializer.DeserializeAsync<MusicInfoes>(musicInfoFile);
		var attributesData = await JsonSerializer.DeserializeAsync<MusicAttributes>(musicAttributeFile);
		var danData = await JsonSerializer.DeserializeAsync<List<DanData>>(danDataFile);
		var gaidenData = await JsonSerializer.DeserializeAsync<List<DanData>>(gaidenDataFile);
		var introData = await JsonSerializer.DeserializeAsync<List<SongIntroductionData>>(songIntroDataFile);
		var movieData = await JsonSerializer.DeserializeAsync<List<MovieData>>(movieDataFile);
		var eventFolderData = await JsonSerializer.DeserializeAsync<List<EventFolderData>>(eventFolderDataFile);
		var shopFolderData = await JsonSerializer.DeserializeAsync<List<ShopFolderData>>(shopFolderDataFile);
		var tokenData = await JsonSerializer.DeserializeAsync<Dictionary<string, int>>(tokenDataFile);
		var lockedSongsData = await JsonSerializer.DeserializeAsync<Dictionary<string, uint[]>>(lockedSongsDataFile);
		var qrCodeData = await JsonSerializer.DeserializeAsync<List<QRCodeData>>(qrCodeDataFile);

		InitializeMusicInfoes(infoesData);

		InitializeMusicAttributes(attributesData);

		InitializeDanData(danData);

		InitializeGaidenData(gaidenData);

		InitializeIntroData(introData);

		InitializeMovieData(movieData);

		InitializeEventFolderData(eventFolderData);

		InitializeShopFolderData(shopFolderData);

		InitializeTokenData(tokenData);

		InitializeLockedSongsData(lockedSongsData);

		InitializeQRCodeData(qrCodeData);
	}

	private static void TryDecompressMusicInfo()
	{
		var dataPath = PathHelper.GetDataPath();
		var musicInfoPath = Path.Combine(dataPath, Constants.MUSIC_INFO_FILE_NAME);
		var compressedMusicInfoPath = Path.Combine(dataPath, Constants.MUSIC_INFO_COMPRESSED_FILE_NAME);

		using var compressed = File.Open(compressedMusicInfoPath, FileMode.Open);
		using var output = File.Create(musicInfoPath);

		GZip.Decompress(compressed, output, true);
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

		musicsWithUra = musicInfoes.Where(info => info.Value.starUra > 0)
			.Select(pair => pair.Key)
			.ToList();
		musicsWithUra.Sort();
	}

	private void InitializeMusicAttributes(MusicAttributes? attributesData)
	{
		attributesData.ThrowIfNull("Shouldn't happen!");

		musicAttributes = attributesData.MusicAttributeEntries.ToImmutableDictionary(attribute => attribute.MusicId);

		musics = musicAttributes.Select(pair => pair.Key)
			.ToList();
		musics.Sort();
	}

	private void InitializeShopFolderData(List<ShopFolderData>? shopFolderData)
	{
		shopFolderData.ThrowIfNull("Shouldn't happen!");
		shopFolderDictionary = shopFolderData.ToImmutableDictionary(data => data.SongNo, ToResponseShopFolderData);
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

	private void InitializeQRCodeData(List<QRCodeData>? qrCodeData)
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