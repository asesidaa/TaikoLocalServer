using System.Collections.Immutable;
using Microsoft.Extensions.Options;
using SharedProject.Models;
using TaikoLocalServer.Models.Application;
using TaikoLocalServer.Settings;

namespace TaikoLocalServer.Handlers;

public record GetInitialDataQuery : IRequest<CommonInitialDataCheckResponse>;

public class GetInitialDataQueryHandler(IGameDataService gameDataService, 
    ILogger<GetInitialDataQueryHandler>                  logger,
    IOptions<ServerSettings>                             settings) 
    : IRequestHandler<GetInitialDataQuery, CommonInitialDataCheckResponse>
{

    private readonly ServerSettings settings = settings.Value;
    
    public Task<CommonInitialDataCheckResponse> Handle(GetInitialDataQuery request, CancellationToken cancellationToken)
    {
        var songIdMax = settings.EnableMoreSongs ? Constants.MUSIC_ID_MAX_EXPANDED : Constants.MUSIC_ID_MAX;

        var musicList = gameDataService.GetMusicList();
        var lockedSongsList = gameDataService.GetLockedSongsList();

        var enabledArray =
            FlagCalculator.GetBitArrayFromIds(musicList, songIdMax, logger);

        var defaultSongList = musicList.Except(lockedSongsList);
        var defaultSongFlg =
            FlagCalculator.GetBitArrayFromIds(defaultSongList, songIdMax, logger);

        var defaultSongWithUraList = gameDataService.GetMusicWithUraList();
        var uraReleaseBit =
            FlagCalculator.GetBitArrayFromIds(defaultSongWithUraList, songIdMax, logger);

        var response = new CommonInitialDataCheckResponse
        {
            Result = 1,
            DefaultSongFlg = defaultSongFlg,
            AchievementSongBit = enabledArray,
            UraReleaseBit = uraReleaseBit,
            SongIntroductionEndDatetime = DateTime.Now.AddYears(10).ToString(Constants.DATE_TIME_FORMAT),
            ServerCurrentDatetime = (ulong)DateTimeOffset.Now.ToUnixTimeSeconds()
        };

        var movieDataDictionary = gameDataService.GetMovieDataDictionary();
        foreach (var movieData in movieDataDictionary)
        {
            response.AryMovieInfoes.Add(movieData.Value);
        }

        // TODO: Figure out what they are individually
        var verupNo1 = new uint[] { 2, 3, 4, 5, 6, 7, 8, 13, 15, 24, 25, 26, 27, 28, 29, 30, 31, 104 };
        var aryVerUp = verupNo1.Select(i => new CommonInitialDataCheckResponse.VerupNoData1
        {
            MasterType = i,
            VerupNo = 1
        }).ToList();
        response.AryVerupNoData1s.AddRange(aryVerUp);
        
        var commonDanDataDictionary = gameDataService.GetCommonDanDataDictionary();
        var commonGaidenDataDictionary = gameDataService.GetCommonGaidenDataDictionary();
        var eventFolderDictionary = gameDataService.GetEventFolderDictionary();
        var songIntroDictionary = gameDataService.GetSongIntroductionDictionary();

        CommonInitialDataCheckResponse.VerupNoData2[] verupNo2List =
        [
            GetVerupNoData2(Constants.DAN_VERUP_MASTER_TYPE, commonDanDataDictionary),
            GetVerupNoData2(Constants.GAIDEN_VERUP_MASTER_TYPE, commonGaidenDataDictionary),
            GetVerupNoData2(Constants.FOLDER_VERUP_MASTER_TYPE, eventFolderDictionary),
            GetVerupNoData2(Constants.INTRO_VERUP_MASTER_TYPE, songIntroDictionary)
        ];
        response.AryVerupNoData2s.AddRange(verupNo2List);

        response.AryChassisFunctionIds = 
        [
            Constants.FUNCTION_ID_DANI_AVAILABLE,
            Constants.FUNCTION_ID_DANI_FOLDER_AVAILABLE,
            Constants.FUNCTION_ID_AI_BATTLE_AVAILABLE
        ];

        return Task.FromResult(response);
    }
    
    private CommonInitialDataCheckResponse.VerupNoData2 GetVerupNoData2<T>(uint masterType, ImmutableDictionary<uint, T> dictionary) 
        where T:IVerupNo
    {
        var infoData = dictionary.Select(pair => new CommonInitialDataCheckResponse.VerupNoData2.InformationData
        {
            InfoId = pair.Key,
            VerupNo = pair.Value.VerupNo
        }).ToList();
        return new CommonInitialDataCheckResponse.VerupNoData2
        {
            MasterType = masterType,
            AryInformationDatas = infoData
        };
    }
    
    
}