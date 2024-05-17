﻿using System.Collections.Immutable;
using TaikoWebUI.Shared.Models;

namespace TaikoWebUI.Services;

public interface IGameDataService
{
    public Task InitializeAsync(string dataBaseUrl);

    public List<MusicDetail> GetMusicList();

    public string GetMusicNameBySongId(uint songId, string? language = null);

    public string GetMusicArtistBySongId(uint songId, string? language = null);

    public SongGenre GetMusicGenreBySongId(uint songId);

    public int GetMusicIndexBySongId(uint songId);

    public DanData GetDanDataById(uint danId);

    public int GetMusicStarLevel(uint songId, Difficulty difficulty);

    public string GetHeadTitle(uint index);
    public string GetKigurumiTitle(uint index);
    public string GetBodyTitle(uint index);
    public string GetFaceTitle(uint index);
    public string GetPuchiTitle(uint index);

    public List<uint> GetKigurumiUniqueIdList();
    public List<uint> GetHeadUniqueIdList();
    public List<uint> GetBodyUniqueIdList();
    public List<uint> GetFaceUniqueIdList();
    public List<uint> GetPuchiUniqueIdList();
    public List<uint> GetTitleUniqueIdList();
    public List<uint> GetTitlePlateIdList();
    
    public List<uint> GetLockedKigurumiUniqueIdList();
    public List<uint> GetLockedHeadUniqueIdList();
    public List<uint> GetLockedBodyUniqueIdList();
    public List<uint> GetLockedFaceUniqueIdList();
    public List<uint> GetLockedPuchiUniqueIdList();
    public List<uint> GetLockedTitleUniqueIdList();
    public List<uint> GetLockedTitlePlateIdList();

    public ImmutableHashSet<Title> GetTitles();
}