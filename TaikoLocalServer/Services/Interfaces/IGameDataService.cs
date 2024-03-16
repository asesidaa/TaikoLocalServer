using System.Collections.Immutable;
using SharedProject.Models;

namespace TaikoLocalServer.Services.Interfaces;

public interface IGameDataService
{
	public Task InitializeAsync();

	public List<uint> GetMusicList();

	public List<uint> GetMusicWithUraList();

	public ImmutableDictionary<uint, MusicInfoEntry> GetMusicInfoes();
	
	public ImmutableDictionary<uint, SongIntroductionData> GetSongIntroductionDictionary();

	public ImmutableDictionary<uint, MovieData> GetMovieDataDictionary();

	public ImmutableDictionary<uint, EventFolderData> GetEventFolderDictionary();
	
	public ImmutableDictionary<uint, DanData> GetCommonDanDataDictionary();
	
	public ImmutableDictionary<uint, DanData> GetCommonGaidenDataDictionary();

	public List<ShopFolderData> GetShopFolderList();

	public Dictionary<string, int> GetTokenDataDictionary();

	public List<uint> GetLockedSongsList();

	public List<int> GetCostumeFlagArraySizes();

	public int GetTitleFlagArraySize();
	
	public int GetToneFlagArraySize();
	
	public ImmutableDictionary<string, uint> GetQRCodeDataDictionary();
}

