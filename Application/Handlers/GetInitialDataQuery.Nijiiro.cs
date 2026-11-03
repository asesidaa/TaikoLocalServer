using System.Collections.Immutable;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetInitialDataQueryHandler
{
    private static readonly uint[] NijiiroGenericMasterTypes = [2, 3, 4, 5, 6, 7, 8, 13, 15, 24, 25, 26, 27, 28, 29, 30, 31];

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

        var cabinet = cabinets.Find(request.ChassisId) is { OwnSettings: true } own ? own : null;
        var movieDataDictionary = gameDataService.Nijiiro().GetMovieDataDictionary();
        if (cabinet is not null)
        {
            // Its own movie list; enable_days as the defaults have it, else 0.
            movieDataDictionary = cabinet.Movies.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(uint.Parse)
                .ToImmutableDictionary(id => id, id => movieDataDictionary.TryGetValue(id, out var m) ? m : new MovieData { MovieId = id });
        }
        foreach (var movieData in movieDataDictionary)
        {
            response.AryMovieInfoes.Add(movieData.Value);
        }

        // The generic masters (getgenericmaster) at version 1. The cabinet keeps its own costume, title and other masks
        // for these and resets any it gets no version for; left empty, the 39.06 exe crashes on the next card scan
        // (it masks the player's costume flags with them).
        CommonInitialDataCheckResponse.VerupNoData1[] verupNo1List =
        [
            .. NijiiroGenericMasterTypes.Select(type => new CommonInitialDataCheckResponse.VerupNoData1 { MasterType = type, VerupNo = 1 }),
            GetVerupNoData1(DomainConstants.ShopVerupMasterType, gameDataService.Nijiiro().GetShopFolderVerup()),
        ];
        response.AryVerupNoData1s.AddRange(verupNo1List);
        
        var telopDataDictionary = gameDataService.Nijiiro().GetTelopDataDictionary();
        var commonDanDataDictionary = cabinet is not null ? gameDataService.Nijiiro().GetDanSet(cabinet.DanSet) : gameDataService.Nijiiro().GetCommonDanDataDictionary();
        var commonGaidenDataDictionary = gameDataService.Nijiiro().GetCommonGaidenDataDictionary();
        var eventFolderDictionary = gameDataService.Nijiiro().GetEventFolderDictionary();
        var songIntroDictionary = gameDataService.Nijiiro().GetSongIntroductionDictionary();

        CommonInitialDataCheckResponse.VerupNoData2[] verupNo2List =
        [
            new CommonInitialDataCheckResponse.VerupNoData2
            {
                MasterType = DomainConstants.TelopVerupMasterType,
                AryInformationDatas = [new() { InfoId = NijiiroTelop.CabinetId, VerupNo = NijiiroTelop.Version(NijiiroTelop.Live(telopDataDictionary.Values, DateTime.Now)) }],
            },
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
