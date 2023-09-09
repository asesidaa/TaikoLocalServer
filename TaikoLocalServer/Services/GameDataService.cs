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

	private ImmutableDictionary<uint, GetSongIntroductionResponse.SongIntroductionData> introDataDictionary =
		ImmutableDictionary<uint, GetSongIntroductionResponse.SongIntroductionData>.Empty;

	private ImmutableDictionary<uint, MusicAttributeEntry> musicAttributes =
		ImmutableDictionary<uint, MusicAttributeEntry>.Empty;

	private ImmutableDictionary<uint, GetfolderResponse.EventfolderData> folderDictionary =
		ImmutableDictionary<uint, GetfolderResponse.EventfolderData>.Empty;

	private List<uint> musics = new();

	private List<uint> musicsWithUra = new();

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

	public ImmutableDictionary<uint, GetSongIntroductionResponse.SongIntroductionData> GetSongIntroDictionary()
	{
		return introDataDictionary;
	}

	public ImmutableDictionary<uint, GetfolderResponse.EventfolderData> GetFolderDictionary()
	{
		return folderDictionary;
	}

	public async Task InitializeAsync()
	{
		var dataPath = PathHelper.GetDataPath();
		var musicInfoPath = Path.Combine(dataPath, Constants.MUSIC_INFO_FILE_NAME);
		var compressedMusicInfoPath = Path.Combine(dataPath, Constants.MUSIC_INFO_COMPRESSED_FILE_NAME);
		var musicAttributePath = Path.Combine(dataPath, Constants.MUSIC_ATTRIBUTE_FILE_NAME);
		var compressedMusicAttributePath = Path.Combine(dataPath, Constants.MUSIC_ATTRIBUTE_COMPRESSED_FILE_NAME);
		var danDataPath = Path.Combine(dataPath, settings.DanDataFileName);
		var songIntroDataPath = Path.Combine(dataPath, settings.IntroDataFileName);
		var eventFolderDataPath = Path.Combine(dataPath, settings.EventFolderDataFileName);

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
		await using var songIntroDataFile = File.OpenRead(songIntroDataPath);
		await using var eventFolderDataFile = File.OpenRead(eventFolderDataPath);

		var infoesData = await JsonSerializer.DeserializeAsync<MusicInfoes>(musicInfoFile);
		var attributesData = await JsonSerializer.DeserializeAsync<MusicAttributes>(musicAttributeFile);
		var danData = await JsonSerializer.DeserializeAsync<List<DanData>>(danDataFile);
		var introData = await JsonSerializer.DeserializeAsync<List<SongIntroductionData>>(songIntroDataFile);
		var eventFolderData = await JsonSerializer.DeserializeAsync<List<EventFolderData>>(eventFolderDataFile);

		InitializeMusicInfoes(infoesData);

		InitializeMusicAttributes(attributesData);

		InitializeDanData(danData);

		InitializeIntroData(introData);

		InitializeEventFolderData(eventFolderData);
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
}