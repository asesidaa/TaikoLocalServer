﻿using System.Collections.Immutable;
using SharedProject.Models;

namespace TaikoLocalServer.Services.Interfaces;

public interface IGameDataService
{
	public Task InitializeAsync();

	public List<uint> GetMusicList();

	public List<uint> GetMusicWithUraList();

	public ImmutableDictionary<uint, MusicInfoEntry> GetMusicInfoes();

	public ImmutableDictionary<uint, MusicAttributeEntry> GetMusicAttributes();

	public ImmutableDictionary<uint, GetDanOdaiResponse.OdaiData> GetDanDataDictionary();
	
	public ImmutableDictionary<uint, GetDanOdaiResponse.OdaiData> GetGaidenDataDictionary();

	public ImmutableDictionary<uint, GetSongIntroductionResponse.SongIntroductionData> GetSongIntroDictionary();

	public ImmutableDictionary<uint, InitialdatacheckResponse.MovieData> GetMovieDataDictionary();

	public ImmutableDictionary<uint, GetfolderResponse.EventfolderData> GetFolderDictionary();

	public List<GetShopFolderResponse.ShopFolderData> GetShopFolderList();

	public Dictionary<string, int> GetTokenDataDictionary();

	public List<uint> GetLockedSongsList();
	
	public ImmutableDictionary<string, uint> GetQRCodeDataDictionary();
}

