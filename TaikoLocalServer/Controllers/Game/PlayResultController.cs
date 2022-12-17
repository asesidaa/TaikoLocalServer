using System.Buffers.Binary;
using System.Globalization;
using System.Text.Json;
using GameDatabase.Entities;
using Throw;

namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r03/chassis/playresult.php")]
[ApiController]
public class PlayResultController : BaseController<PlayResultController>
{
    private readonly IAiDatumService aiDatumService;

    private readonly IDanScoreDatumService danScoreDatumService;

    private readonly ISongBestDatumService songBestDatumService;

    private readonly ISongPlayDatumService songPlayDatumService;
    private readonly IUserDatumService userDatumService;

    public PlayResultController(IUserDatumService userDatumService, ISongPlayDatumService songPlayDatumService,
        ISongBestDatumService songBestDatumService, IDanScoreDatumService danScoreDatumService,
        IAiDatumService aiDatumService)
    {
        this.userDatumService = userDatumService;
        this.songPlayDatumService = songPlayDatumService;
        this.songBestDatumService = songBestDatumService;
        this.danScoreDatumService = danScoreDatumService;
        this.aiDatumService = aiDatumService;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> UploadPlayResult([FromBody] PlayResultRequest request)
    {
        Logger.LogInformation("PlayResult request : {Request}", request.Stringify());
        var decompressed = GZipBytesUtil.DecompressGZipBytes(request.PlayresultData);

        var playResultData = Serializer.Deserialize<PlayResultDataRequest>(new ReadOnlySpan<byte>(decompressed));

        Logger.LogInformation("Play result data {Data}", playResultData.Stringify());

        var response = new PlayResultResponse
        {
            Result = 1
        };

        // Fix issue caused by guest play, god knows why they send guest play data
        if (request.BaidConf == 0) return Ok(response);

        if (await userDatumService.GetFirstUserDatumOrNull(request.BaidConf) is null)
        {
            Logger.LogWarning("Game uploading a non existing user with baid {Baid}", request.BaidConf);
            return Ok(response);
        }

        var lastPlayDatetime = DateTime.ParseExact(playResultData.PlayDatetime, Constants.DATE_TIME_FORMAT,
            CultureInfo.InvariantCulture);

        await UpdateUserData(request, playResultData, lastPlayDatetime);

        var playMode = (PlayMode)playResultData.PlayMode;

        if (playMode == PlayMode.DanMode)
        {
            await UpdateDanPlayData(request, playResultData);
            return Ok(response);
        }

        var bestData = await songBestDatumService.GetAllSongBestData(request.BaidConf);
        for (var songNumber = 0; songNumber < playResultData.AryStageInfoes.Count; songNumber++)
        {
            var stageData = playResultData.AryStageInfoes[songNumber];

            if (stageData.IsSkipUse)
            {
                await UpdatePlayData(request, songNumber, stageData, lastPlayDatetime);
                continue;
            }

            if (playMode == PlayMode.AiBattle) await UpdateAiBattleData(request, stageData);

            await UpdateBestData(request, stageData, bestData);

            await UpdatePlayData(request, songNumber, stageData, lastPlayDatetime);
        }

        return Ok(response);
    }

    private async Task UpdateDanPlayData(PlayResultRequest request, PlayResultDataRequest playResultDataRequest)
    {
        if (playResultDataRequest.IsNotRecordedDan)
        {
            Logger.LogInformation("Dan score will not be saved!");
            return;
        }

        var danScoreData =
            await danScoreDatumService.GetSingleDanScoreDatum(request.BaidConf, playResultDataRequest.DanId) ??
            new DanScoreDatum
            {
                Baid = request.BaidConf,
                DanId = playResultDataRequest.DanId
            };
        danScoreData.ClearState =
            (DanClearState)Math.Max(playResultDataRequest.DanResult, (uint)danScoreData.ClearState);
        danScoreData.ArrivalSongCount =
            Math.Max((uint)playResultDataRequest.AryStageInfoes.Count, danScoreData.ArrivalSongCount);
        danScoreData.ComboCountTotal = Math.Max(playResultDataRequest.ComboCntTotal, danScoreData.ComboCountTotal);
        danScoreData.SoulGaugeTotal = Math.Max(playResultDataRequest.SoulGaugeTotal, danScoreData.SoulGaugeTotal);

        UpdateDanStageData(playResultDataRequest, danScoreData);

        await danScoreDatumService.InsertOrUpdateDanScoreDatum(danScoreData);
    }

    private void UpdateDanStageData(PlayResultDataRequest playResultDataRequest, DanScoreDatum danScoreData)
    {
        for (var i = 0; i < playResultDataRequest.AryStageInfoes.Count; i++)
        {
            var stageData = playResultDataRequest.AryStageInfoes[i];

            var songNumber = i;
            var danStageData = danScoreData.DanStageScoreData.FirstOrDefault(datum => datum.SongNumber == songNumber,
                new DanStageScoreDatum
                {
                    Baid = danScoreData.Baid,
                    DanId = danScoreData.DanId,
                    SongNumber = (uint)songNumber,
                    OkCount = stageData.OkCnt,
                    BadCount = stageData.NgCnt
                });

            danStageData.HighScore = Math.Max(danStageData.HighScore, stageData.PlayScore);
            danStageData.ComboCount = Math.Max(danStageData.ComboCount, stageData.ComboCnt);
            danStageData.DrumrollCount = Math.Max(danStageData.DrumrollCount, stageData.PoundCnt);
            danStageData.GoodCount = Math.Max(danStageData.GoodCount, stageData.GoodCnt);
            danStageData.TotalHitCount = Math.Max(danStageData.TotalHitCount, stageData.HitCnt);
            danStageData.OkCount = Math.Min(danStageData.OkCount, stageData.OkCnt);
            danStageData.BadCount = Math.Min(danStageData.BadCount, stageData.NgCnt);

            var index = danScoreData.DanStageScoreData.IndexOf(danStageData);
            if (index == -1) danScoreData.DanStageScoreData.Add(danStageData);
        }
    }

    private async Task UpdatePlayData(PlayResultRequest request, int songNumber,
        PlayResultDataRequest.StageData stageData,
        DateTime lastPlayDatetime)
    {
        var option = BinaryPrimitives.ReadInt16LittleEndian(stageData.OptionFlg);
        var songPlayDatum = new SongPlayDatum
        {
            Baid = request.BaidConf,
            SongNumber = (uint)songNumber,
            GoodCount = stageData.GoodCnt,
            OkCount = stageData.OkCnt,
            MissCount = stageData.NgCnt,
            ComboCount = stageData.ComboCnt,
            HitCount = stageData.HitCnt,
            DrumrollCount = stageData.PoundCnt,
            Crown = PlayResultToCrown(stageData.PlayResult, stageData.OkCnt),
            Score = stageData.PlayScore,
            ScoreRate = stageData.ScoreRate,
            ScoreRank = (ScoreRank)stageData.ScoreRank,
            Skipped = stageData.IsSkipUse,
            SongId = stageData.SongNo,
            PlayTime = lastPlayDatetime,
            Difficulty = (Difficulty)stageData.Level,
            Option = option
        };
        await songPlayDatumService.AddSongPlayDatum(songPlayDatum);
    }

    private async Task UpdateUserData(PlayResultRequest request, PlayResultDataRequest playResultData,
        DateTime lastPlayDatetime)
    {
        var userdata = await userDatumService.GetFirstUserDatumOrNull(request.BaidConf);

        userdata.ThrowIfNull($"User data is null! Baid: {request.BaidConf}");

        var playMode = (PlayMode)playResultData.PlayMode;

        userdata.Title = playResultData.Title;
        userdata.TitlePlateId = playResultData.TitleplateId;
        var costumeData = new List<uint>
        {
            playResultData.AryCurrentCostume.Costume1,
            playResultData.AryCurrentCostume.Costume2,
            playResultData.AryCurrentCostume.Costume3,
            playResultData.AryCurrentCostume.Costume4,
            playResultData.AryCurrentCostume.Costume5
        };
        userdata.CostumeData = JsonSerializer.Serialize(costumeData);

        // Official cabinet does not save option at the end of the game, so I turned it off. -S-Sebb??
        // Skip user setting saving when in dan mode as dan mode uses its own default setting
        // if (playMode != PlayMode.DanMode)
        // {
        //     var lastStage = playResultData.AryStageInfoes.Last();
        //     var option = BinaryPrimitives.ReadInt16LittleEndian(lastStage.OptionFlg);
        //     userdata.OptionSetting = option;
        //     userdata.IsSkipOn = lastStage.IsSkipOn;
        //     userdata.IsVoiceOn = !lastStage.IsVoiceOn;
        //     userdata.NotesPosition = lastStage.NotesPosition;
        // }

        userdata.LastPlayDatetime = lastPlayDatetime;
        userdata.LastPlayMode = playResultData.PlayMode;

        userdata.ToneFlgArray =
            UpdateJsonUintFlagArray(userdata.ToneFlgArray, playResultData.GetToneNoes, nameof(userdata.ToneFlgArray));

        userdata.TitleFlgArray =
            UpdateJsonUintFlagArray(userdata.TitleFlgArray, playResultData.GetTitleNoes,
                nameof(userdata.TitleFlgArray));

        userdata.CostumeFlgArray = UpdateJsonCostumeFlagArray(userdata.CostumeFlgArray,
            new[]
            {
                playResultData.GetCostumeNo1s,
                playResultData.GetCostumeNo2s,
                playResultData.GetCostumeNo3s,
                playResultData.GetCostumeNo4s,
                playResultData.GetCostumeNo5s
            });

        userdata.GenericInfoFlgArray =
            UpdateJsonUintFlagArray(userdata.GenericInfoFlgArray, playResultData.GetGenericInfoNoes,
                nameof(userdata.GenericInfoFlgArray));

        userdata.AiWinCount += playResultData.AryStageInfoes.Count(data => data.IsWin);
        await userDatumService.UpdateUserDatum(userdata);
    }

    private string UpdateJsonUintFlagArray(string originalValue, IReadOnlyCollection<uint>? newValue, string fieldName)
    {
        var flgData = new List<uint>();
        try
        {
            flgData = JsonSerializer.Deserialize<List<uint>>(originalValue);
        }
        catch (JsonException e)
        {
            Logger.LogError(e, "Parsing {FieldName} json data failed", fieldName);
        }

        flgData?.AddRange(newValue ?? Array.Empty<uint>());
        var flgArray = flgData ?? new List<uint>();
        return JsonSerializer.Serialize(flgArray);
    }

    private string UpdateJsonCostumeFlagArray(string originalValue, IReadOnlyList<IReadOnlyCollection<uint>?>? newValue)
    {
        var flgData = new List<List<uint>>();
        try
        {
            flgData = JsonSerializer.Deserialize<List<List<uint>>>(originalValue);
        }
        catch (JsonException e)
        {
            Logger.LogError(e, "Parsing Costume flag json data failed");
        }

        if (flgData is null) flgData = new List<List<uint>>();

        for (var index = 0; index < flgData.Count; index++)
        {
            var subFlgData = flgData[index];
            subFlgData.AddRange(newValue?[index] ?? Array.Empty<uint>());
        }

        if (flgData.Count >= 5) return JsonSerializer.Serialize(flgData);

        Logger.LogWarning("Costume flag array count less than 5!");
        flgData = new List<List<uint>>
        {
            new(), new(), new(), new(), new()
        };

        return JsonSerializer.Serialize(flgData);
    }

    private async Task UpdateBestData(PlayResultRequest request, PlayResultDataRequest.StageData stageData,
        IEnumerable<SongBestDatum> bestData)
    {
        var bestDatum = bestData
            .FirstOrDefault(datum => datum.SongId == stageData.SongNo &&
                                     datum.Difficulty == (Difficulty)stageData.Level,
                new SongBestDatum
                {
                    Baid = request.BaidConf,
                    SongId = stageData.SongNo,
                    Difficulty = (Difficulty)stageData.Level
                });

        // Determine whether it is dondaful crown as this is not reflected by play result
        var crown = PlayResultToCrown(stageData.PlayResult, stageData.OkCnt);

        var option = BinaryPrimitives.ReadInt16LittleEndian(stageData.OptionFlg);
        bestDatum.UpdateBestData(crown, stageData.ScoreRank, stageData.PlayScore, stageData.ScoreRate, option);

        await songBestDatumService.UpdateOrInsertSongBestDatum(bestDatum);
    }

    private async Task UpdateAiBattleData(PlayResultRequest request, PlayResultDataRequest.StageData stageData)
    {
        var difficulty = (Difficulty)stageData.Level;
        difficulty.Throw().IfOutOfRange();
        var existing = await aiDatumService.GetSongAiScore(request.BaidConf,
            stageData.SongNo, difficulty);

        if (existing is null)
        {
            var aiScoreDatum = new AiScoreDatum
            {
                Baid = request.BaidConf,
                SongId = stageData.SongNo,
                Difficulty = difficulty,
                IsWin = stageData.IsWin
            };
            for (var index = 0; index < stageData.ArySectionDatas.Count; index++)
                AddNewAiSectionScore(request, stageData, index, difficulty, aiScoreDatum);

            await aiDatumService.InsertSongAiScore(aiScoreDatum);
            return;
        }

        for (var index = 0; index < stageData.ArySectionDatas.Count; index++)
        {
            var sectionData = stageData.ArySectionDatas[index];
            if (index < existing.AiSectionScoreData.Count)
                existing.AiSectionScoreData[index].UpdateBest(sectionData);
            else
                AddNewAiSectionScore(request, stageData, index, difficulty, existing);
        }

        await aiDatumService.UpdateSongAiScore(existing);
    }

    private static void AddNewAiSectionScore(PlayResultRequest request, PlayResultDataRequest.StageData stageData,
        int index, Difficulty difficulty,
        AiScoreDatum aiScoreDatum)
    {
        var sectionData = stageData.ArySectionDatas[index];
        var aiSectionScoreDatum = new AiSectionScoreDatum
        {
            Baid = request.BaidConf,
            SongId = stageData.SongNo,
            Difficulty = difficulty,
            SectionIndex = index,
            OkCount = sectionData.OkCnt,
            MissCount = sectionData.NgCnt
        };
        aiSectionScoreDatum.UpdateBest(sectionData);
        aiScoreDatum.AiSectionScoreData.Add(aiSectionScoreDatum);
    }


    private static CrownType PlayResultToCrown(uint playResult, uint okCount)
    {
        var crown = (CrownType)playResult;
        if (crown == CrownType.Gold && okCount == 0) crown = CrownType.Dondaful;

        return crown;
    }
}