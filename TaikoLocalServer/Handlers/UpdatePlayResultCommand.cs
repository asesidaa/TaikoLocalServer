using System.Text.Json;
using GameDatabase.Context;
using GameDatabase.Entities;
using TaikoLocalServer.Models.Application;
using Throw;

namespace TaikoLocalServer.Handlers;

public record UpdatePlayResultCommand(uint Baid, CommonPlayResultData PlayResultData) : IRequest<uint>;

public class UpdatePlayResultCommandHandler(TaikoDbContext context, ILogger<UpdatePlayResultCommandHandler> logger)
    : IRequestHandler<UpdatePlayResultCommand, uint>
{
    public async Task<uint> Handle(UpdatePlayResultCommand request, CancellationToken cancellationToken)
    {
        if (request.Baid == 0)
        {
            return 1;
        }

        var user = await context.UserData.FindAsync([request.Baid], cancellationToken);
        if (user is null)
        {
            logger.LogWarning("Game uploading a non exisiting user with baid {Baid}", request.Baid);
            return 1;
        }

        var lastPlayDateTime = DateTime.Now;
        var playResultData = request.PlayResultData;
        UpdateUserData(user, playResultData, lastPlayDateTime);
        
        var playMode = (PlayMode)playResultData.PlayMode;
        if (playMode is PlayMode.DanMode or PlayMode.GaidenMode)
        {
            var danType = playMode == PlayMode.DanMode ? DanType.Normal : DanType.Gaiden;
            var danPlayData = await context.DanScoreData
                .Include(datum => datum.DanStageScoreData)
                .FirstOrDefaultAsync(datum => datum.Baid == request.Baid &&
                                              datum.DanId == playResultData.DanId &&
                                              datum.DanType == danType, cancellationToken);
            if (danPlayData is null)
            {
                danPlayData = new DanScoreDatum
                {
                    Baid = request.Baid,
                    DanId = playResultData.DanId,
                    DanType = danType
                };
                UpdateDanPlayData(danPlayData, playResultData);
                context.DanScoreData.Add(danPlayData);
            }
            else
            {
                UpdateDanPlayData(danPlayData, playResultData);
            }

            await context.SaveChangesAsync(cancellationToken);
            return 1;
        }
        
        for (var songNumber = 0; songNumber < playResultData.AryStageInfoes.Count; songNumber++)
        {
            var stageData = playResultData.AryStageInfoes[songNumber];

            if (playMode == PlayMode.AiBattle)
            {
                await UpdateAiBattleData(playResultData, stageData);
            }

            var difficulty = (Difficulty)stageData.Level;
            difficulty.Throw().IfOutOfRange();
            var existing = await context.SongBestData.FindAsync([playResultData.Baid, stageData.SongNo, difficulty], cancellationToken);

            // Determine whether it is dondaful crown as this is not reflected by play result
            var crown = PlayResultToCrown(stageData.PlayResult, stageData.OkCnt);

            if (existing is null)
            {
                var datum = new SongBestDatum
                {
                    Baid = playResultData.Baid,
                    SongId = stageData.SongNo,
                    Difficulty = difficulty,
                    BestScore = stageData.PlayScore,
                    BestRate = stageData.ScoreRate,
                    BestCrown = crown,
                    BestScoreRank = (ScoreRank)stageData.ScoreRank
                };

                context.SongBestData.Add(datum);
            }
            else
            {
                existing.UpdateBestData(crown, stageData.ScoreRank, stageData.PlayScore, stageData.ScoreRate);
            }

            var songPlayDatum = new SongPlayDatum
            {
                Baid = request.Baid,
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
                PlayTime = lastPlayDateTime,
                Difficulty = (Difficulty)stageData.Level
            };
            context.SongPlayData.Add(songPlayDatum);
        }

        await context.SaveChangesAsync(cancellationToken);
        return 1;
    }

    private async Task UpdateAiBattleData(CommonPlayResultData playResultData, CommonPlayResultData.StageData stageData)
    {
        var difficulty = (Difficulty)stageData.Level;
        difficulty.Throw().IfOutOfRange();
        var existing = await context.AiScoreData
            .Include(datum => datum.AiSectionScoreData)
            .FirstOrDefaultAsync(datum => datum.Baid == playResultData.Baid &&
                                          datum.SongId == stageData.SongNo &&
                                          datum.Difficulty == difficulty)
            ?? context.AiScoreData.Local.FirstOrDefault(datum => datum.Baid == playResultData.Baid &&
                                                                             datum.SongId == stageData.SongNo &&
                                                                             datum.Difficulty == difficulty);
        if (existing is null)
        {
            existing = new AiScoreDatum
            {
                Baid = playResultData.Baid,
                SongId = stageData.SongNo,
                Difficulty = difficulty,
                IsWin = stageData.IsWin
            };
            var aiSections = stageData.ArySectionDatas.Select((data, i) =>
                {
                    var section = new AiSectionScoreDatum
                    {
                        Baid = playResultData.Baid,
                        SongId = stageData.SongNo,
                        Difficulty = difficulty,
                        SectionIndex = i,
                        OkCount = data.OkCnt,
                        MissCount = data.NgCnt
                    };
                    section.UpdateBest(data);
                    return section;
                }
            );
            existing.AiSectionScoreData.AddRange(aiSections);
            context.AiScoreData.Add(existing);
            return;
        }
        
        for (var index = 0; index < stageData.ArySectionDatas.Count; index++)
        {
            var sectionData = stageData.ArySectionDatas[index];
            if (index < existing.AiSectionScoreData.Count)
            {
                existing.AiSectionScoreData[index].UpdateBest(sectionData);
            }
            else
            {
                var aiSectionScoreDatum = new AiSectionScoreDatum
                {
                    Baid = playResultData.Baid,
                    SongId = stageData.SongNo,
                    Difficulty = difficulty,
                    SectionIndex = index,
                    OkCount = sectionData.OkCnt,
                    MissCount = sectionData.NgCnt
                };
                aiSectionScoreDatum.UpdateBest(sectionData);
                existing.AiSectionScoreData.Add(aiSectionScoreDatum);
            }
        }
    }
    
    private void UpdateDanPlayData(DanScoreDatum danPlayData, CommonPlayResultData playResultData)
    {
        danPlayData.ClearState =
            (DanClearState)Math.Max(playResultData.DanResult, (uint)danPlayData.ClearState);
        danPlayData.ArrivalSongCount =
            Math.Max((uint)playResultData.AryStageInfoes.Count, danPlayData.ArrivalSongCount);
        danPlayData.ComboCountTotal = Math.Max(playResultData.ComboCntTotal, danPlayData.ComboCountTotal);
        danPlayData.SoulGaugeTotal = Math.Max(playResultData.SoulGaugeTotal, danPlayData.SoulGaugeTotal);
        
        for (var i = 0; i < playResultData.AryStageInfoes.Count; i++)
        {
            var stageData = playResultData.AryStageInfoes[i];

            var songNumber = i;
            var danStageData = danPlayData.DanStageScoreData.FirstOrDefault(datum => datum.SongNumber == songNumber,
                new DanStageScoreDatum
                {
                    Baid = danPlayData.Baid,
                    DanId = danPlayData.DanId,
                    DanType = danPlayData.DanType,
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

            var index = danPlayData.DanStageScoreData.IndexOf(danStageData);
            if (index == -1)
            {
                context.DanStageScoreData.Add(danStageData);
            }
        }
    }

    private void UpdateUserData(UserDatum user, CommonPlayResultData playResultData, DateTime lastPlayDateTime)
    {
        user.Title = playResultData.Title;
        user.TitlePlateId = playResultData.TitleplateId;
        var costumeData = new List<uint>
        {
            playResultData.AryCurrentCostume.Costume1,
            playResultData.AryCurrentCostume.Costume2,
            playResultData.AryCurrentCostume.Costume3,
            playResultData.AryCurrentCostume.Costume4,
            playResultData.AryCurrentCostume.Costume5
        };
        //user.CostumeData = JsonSerializer.Serialize(costumeData);
        user.CurrentKigurumi = playResultData.AryCurrentCostume.Costume1;
        user.CurrentHead = playResultData.AryCurrentCostume.Costume2;
        user.CurrentBody = playResultData.AryCurrentCostume.Costume3;
        user.CurrentFace = playResultData.AryCurrentCostume.Costume4;
        user.CurrentPuchi = playResultData.AryCurrentCostume.Costume5;
        user.LastPlayDatetime = lastPlayDateTime;
        user.LastPlayMode = playResultData.PlayMode;

        user.ToneFlgArray.AddRange(playResultData.GetToneNoes);
        user.TitleFlgArray.AddRange(playResultData.GetTitleNoes);

        user.UnlockedKigurumi.AddRange(playResultData.GetCostumeNo1s);
        user.UnlockedHead.AddRange(playResultData.GetCostumeNo2s);
        user.UnlockedBody.AddRange(playResultData.GetCostumeNo3s);
        user.UnlockedFace.AddRange(playResultData.GetCostumeNo4s);
        user.UnlockedPuchi.AddRange(playResultData.GetCostumeNo5s);
        var genericInfo = user.GenericInfoFlgArray.ToList();
        genericInfo.AddRange(playResultData.GetGenericInfoNoes);
        user.GenericInfoFlgArray = genericInfo.ToArray();

        var difficultyPlayedArray = new List<uint>
        {
            playResultData.DifficultyPlayedCourse,
            playResultData.DifficultyPlayedStar,
            playResultData.DifficultyPlayedSort
        };
        //user.DifficultyPlayedArray = JsonSerializer.Serialize(difficultyPlayedArray);
        user.DifficultyPlayedCourse = playResultData.DifficultyPlayedCourse;
        user.DifficultyPlayedStar = playResultData.DifficultyPlayedStar;
        user.DifficultyPlayedSort = playResultData.DifficultyPlayedSort;

        user.AiWinCount += playResultData.AryStageInfoes.Count(data => data.IsWin);
    }
    
    private static CrownType PlayResultToCrown(uint playResult, uint okCount)
    {
        var crown = (CrownType)playResult;
        if (crown == CrownType.Gold && okCount == 0)
        {
            crown = CrownType.Dondaful;
        }

        return crown;
    }
}