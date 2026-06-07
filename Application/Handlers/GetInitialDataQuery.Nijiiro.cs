using System.Collections.Immutable;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetInitialDataQueryHandler
{
    private partial ValueTask<CommonInitialDataCheckResponse> HandleNijiiro(GetInitialDataQuery request, CancellationToken cancellationToken)
    {
        var songIdMax = settings.EnableMoreSongs ? settings.MoreSongsSize : DomainConstants.MusicIdMax;

        var musicList = gameDataService.Nijiiro().GetMusicList();
        var lockedSongsList = gameDataService.Nijiiro().GetLockedSongsList();
        var lockedUraSongsList = gameDataService.Nijiiro().GetLockedUraSongsList();

        var enabledArray =
            FlagCalculator.GetBitArrayFromIds(musicList, songIdMax, logger);

        var defaultSongList = musicList.Except(lockedSongsList);
        var defaultSongFlg =
            FlagCalculator.GetBitArrayFromIds(defaultSongList, songIdMax, logger);

        var defaultSongWithUraList = gameDataService.Nijiiro().GetMusicWithUraList().Except(lockedUraSongsList);
        var uraReleaseBit =
            FlagCalculator.GetBitArrayFromIds(defaultSongWithUraList, songIdMax, logger);

        var response = new CommonInitialDataCheckResponse
        {
            Result = 1,
            DefaultSongFlg = defaultSongFlg,
            AchievementSongBit = enabledArray,
            UraReleaseBit = uraReleaseBit,
            SongIntroductionEndDatetime = DateTime.Now.AddYears(10).ToString(Constants.DateTimeFormat),
            ServerCurrentDatetime = (ulong)DateTimeOffset.Now.ToUnixTimeSeconds()
        };

        var movieDataDictionary = gameDataService.Nijiiro().GetMovieDataDictionary();
        foreach (var movieData in movieDataDictionary)
        {
            response.AryMovieInfoes.Add(movieData.Value);
        }

        CommonInitialDataCheckResponse.VerupNoData1[] verupNo1List =
        [
            GetVerupNoData1(DomainConstants.ShopVerupMasterType, gameDataService.Nijiiro().GetShopFolderVerup()),
        ];
        response.AryVerupNoData1s.AddRange(verupNo1List);
        
        var commonDanDataDictionary = gameDataService.Nijiiro().GetCommonDanDataDictionary();
        var commonGaidenDataDictionary = gameDataService.Nijiiro().GetCommonGaidenDataDictionary();
        var eventFolderDictionary = gameDataService.Nijiiro().GetEventFolderDictionary();
        var songIntroDictionary = gameDataService.Nijiiro().GetSongIntroductionDictionary();

        CommonInitialDataCheckResponse.VerupNoData2[] verupNo2List =
        [
            GetVerupNoData2(DomainConstants.DanVerupMasterType, commonDanDataDictionary),
            GetVerupNoData2(DomainConstants.GaidenVerupMasterType, commonGaidenDataDictionary),
            GetVerupNoData2(DomainConstants.FolderVerupMasterType, eventFolderDictionary),
            GetVerupNoData2(DomainConstants.IntroVerupMasterType, songIntroDictionary)
        ];
        response.AryVerupNoData2s.AddRange(verupNo2List);

        response.AryChassisFunctionIds = 
        [
            DomainConstants.FunctionIdDaniAvailable,
            DomainConstants.FunctionIdDaniFolderAvailable,
            DomainConstants.FunctionIdAiBattleAvailable
        ];

        return ValueTask.FromResult(response);
    }

    private CommonInitialDataCheckResponse.VerupNoData1 GetVerupNoData1(uint masterType, uint verup)
    {
        return new CommonInitialDataCheckResponse.VerupNoData1
        {
            MasterType = masterType,
            VerupNo = verup
        };
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
