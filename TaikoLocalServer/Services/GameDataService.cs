using System.Collections.Immutable;
using System.Text.Json;
using SharedProject.Models;
using Swan.Mapping;
using TaikoLocalServer.Services.Interfaces;
using Throw;

namespace TaikoLocalServer.Services;

public class GameDataService : IGameDataService
{
    private ImmutableDictionary<uint, GetDanOdaiResponse.OdaiData> danDataDictionary = 
        ImmutableDictionary<uint, GetDanOdaiResponse.OdaiData>.Empty;

    private ImmutableDictionary<uint, GetSongIntroductionResponse.SongIntroductionData> introDataDictionary = 
        ImmutableDictionary<uint, GetSongIntroductionResponse.SongIntroductionData>.Empty;
    
    private ImmutableDictionary<uint, MusicAttributeEntry> musicAttributes = 
        ImmutableDictionary<uint, MusicAttributeEntry>.Empty;

    private List<uint> musics = new();

    private List<uint> musicsWithUra = new();

    public List<uint> GetMusicList()
    {
        return musics;
    }

    public List<uint> GetMusicWithUraList()
    {
        return musicsWithUra;
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

    public async Task InitializeAsync()
    {
        var dataPath = PathHelper.GetDataPath();
        var musicAttributePath = Path.Combine(dataPath, Constants.MUSIC_ATTRIBUTE_FILE_NAME);
        var danDataPath = Path.Combine(dataPath, Constants.DAN_DATA_FILE_NAME);
        var songIntroDataPath = Path.Combine(dataPath, Constants.INTRO_DATA_FILE_NAME);

        await using var musicAttributeFile = File.OpenRead(musicAttributePath);
        await using var danDataFile = File.OpenRead(danDataPath);
        await using var songIntroDataFile = File.OpenRead(songIntroDataPath);

        var attributesData = await JsonSerializer.DeserializeAsync<MusicAttributes>(musicAttributeFile);
        var danData = await JsonSerializer.DeserializeAsync<List<DanData>>(danDataFile);
        var introData = await JsonSerializer.DeserializeAsync<List<SongIntroductionData>>(songIntroDataFile);

        InitializeMusicAttributes(attributesData);

        InitializeDanData(danData);

        InitializeIntroData(introData);
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
}