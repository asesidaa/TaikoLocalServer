using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers;

[Mapper]
public static partial class PlayResultMappers
{
    public static CommonPlayResultData Map(PlayResultRequest request)
    {
        return new CommonPlayResultData
        {
            Baid = request.Baid,
            ChassisId = request.ChassisId,
            ShopId = request.ShopId,
            PlayDatetime = request.PlayDatetime,
            IsRight = request.IsRight,
            CardType = request.CardType,
            IsTwoPlayers = request.IsTwoPlayers,
            AryStageInfoes = request.AryStageInfoes.Select(MapStage).ToList(),
            ReleaseSongNoes = (request.ReleaseSongNoes ?? []).ToList(),
            GetToneNoes = (request.GetToneNoes ?? []).ToList(),
            GetCostumeNo1s = (request.GetCostumeNo1s ?? []).ToList(),
            GetCostumeNo2s = (request.GetCostumeNo2s ?? []).ToList(),
            GetCostumeNo3s = (request.GetCostumeNo3s ?? []).ToList(),
            GetCostumeNo4s = (request.GetCostumeNo4s ?? []).ToList(),
            GetCostumeNo5s = (request.GetCostumeNo5s ?? []).ToList(),
            GetTitleNoes = (request.GetTitleNoes ?? []).ToList(),
            GetDonmedal = request.GetDonmedal,
            GetKatsumedal = request.GetKatsumedal,
            BonusDailyFlg = request.BonusDailyFlg,
            BonusWeeklyFlg = request.BonusWeeklyFlg,
            BonusMonthlyFlg = request.BonusMonthlyFlg,
            ItemshopTutorialFlg = request.ItemshopTutorialFlg,
            IsDevil = request.IsDevil,
            IsExplain = request.IsExplain,
            AryPlayCostume = MapCostume(request.AryPlayCostume),
            AryCurrentCostume = MapCostume(request.AryCurrentCostume),
            HasAryCurrentCostume = request.AryCurrentCostume is not null,
            GenderType = request.GenderType,
            PlayerAge = request.PlayerAge,
            PlayMode = request.PlayMode,
            IsTokkunPlayResult = request.PlayMode == (uint)PlayMode.Tokkun,
            TokkunTutorialFlg = request.TokkunTutorialFlg,
            TokkunStageData = MapTokkunStageData(request.AryTokkunstageInfo),
            AreaCode = request.AreaCode,
            Reserved = request.Reserved ?? [],
            LowerlimitAge = request.LowerlimitAge,
            UpperlimitAge = request.UpperlimitAge,
            AgeScore = request.AgeScore,
            EstimationCount = request.EstimationCount,
            DanResult = request.DanResult.GetValueOrDefault(),
            AryCollaboInfo = request.AryCollaboInfoes.Select(MapCollabo).ToList(),
            TournamentMode = request.TournamentMode.GetValueOrDefault(),
            Accesstoken = request.Accesstoken ?? string.Empty,
            ContentInfo = request.ContentInfo ?? [],
            DifficultyPlayedCourse = request.DifficultyPlayedCourse.GetValueOrDefault(),
            DifficultyPlayedStar = request.DifficultyPlayedStar.GetValueOrDefault(),
            HasDifficultyPlayedCourse = request.DifficultyPlayedCourse is not null,
            HasDifficultyPlayedStar = request.DifficultyPlayedStar is not null,
            WaiwaiTutorialFlg = request.WaiwaiTutorialFlg
        };
    }

    public static PlayResultResponse Map(uint result)
        => new() { Result = result };

    private static CommonPlayResultData.StageData MapStage(PlayResultRequest.StageData stage)
    {
        return new CommonPlayResultData.StageData
        {
            SongNo = stage.SongNo,
            Level = stage.Level,
            PlayResult = stage.PlayResult,
            PlayScore = stage.PlayScore,
            GoodCnt = stage.GoodCnt,
            OkCnt = stage.OkCnt,
            NgCnt = stage.NgCnt,
            PoundCnt = stage.PoundCnt,
            ComboCnt = stage.ComboCnt,
            HitCnt = stage.HitCnt.GetValueOrDefault(),
            OptionFlg = stage.OptionFlg ?? [],
            ToneFlg = stage.ToneFlg ?? [],
            AryChallengeIds = stage.AryChallengeIds.Select(MapCompe).ToList(),
            AryUserCompeIds = stage.AryUserCompeIds.Select(MapCompe).ToList(),
            AryBngCompeIds = stage.AryBngCompeIds.Select(MapCompe).ToList(),
            MusicCateg = stage.MusicCateg,
            IsPushed = stage.IsPushed,
            IsFavorite = stage.IsFavorite,
            IsRecent = stage.IsRecent,
            IsPapamama = stage.IsPapamama,
            PlayDan = stage.PlayDan is > 0 ? stage.PlayDan : null,
            SoulGauge = stage.SoulGauge,
            StageMode = stage.StageMode,
            SelectedFolderId = stage.SelectedFolderId,
            WaiwaiResult = stage.WaiwaiResult,
            WaiwaiGauge = stage.WaiwaiGauge
        };
    }

    private static partial CommonPlayResultData.ResultcompeData MapCompe(
        PlayResultRequest.StageData.ResultcompeData data);

    private static CommonPlayResultData.CostumeData MapCostume(PlayResultRequest.CostumeData? costume)
    {
        return costume is null
            ? new CommonPlayResultData.CostumeData()
            : MapCostumeData(costume);
    }

    private static partial CommonPlayResultData.CostumeData MapCostumeData(PlayResultRequest.CostumeData costume);

    private static partial CommonPlayResultData.CollaboData MapCollabo(PlayResultRequest.CollaboData collabo);

    private static CommonPlayResultData.TokkunStageDataDto? MapTokkunStageData(
        PlayResultRequest.TokkunstageData? data)
        => data is null
            ? null
            : new CommonPlayResultData.TokkunStageDataDto
            {
                BanacoinDatetime = data.BanacoinDatetime ?? string.Empty,
                TokkunSongCnt = data.TokkunSongCnt,
                TookunSongnoes = (data.TookunSongnoes ?? []).ToList(),
                TokkunSpeedchangeCnt = data.TokkunSpeedchangeCnt,
                TokkunAutoplayCnt = data.TokkunAutoplayCnt,
                TokkunJumpCnt = data.TokkunJumpCnt
            };
}
