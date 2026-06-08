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
            ItemshopTutorialFlg = request.ShouldSerializeItemshopTutorialFlg() ? request.ItemshopTutorialFlg : null,
            IsDevil = request.ShouldSerializeIsDevil() ? request.IsDevil : null,
            IsExplain = request.ShouldSerializeIsExplain() ? request.IsExplain : null,
            AryPlayCostume = MapCostume(request.AryPlayCostume),
            AryCurrentCostume = MapCostume(request.AryCurrentCostume),
            HasAryCurrentCostume = request.AryCurrentCostume is not null,
            GenderType = request.GenderType,
            PlayerAge = request.PlayerAge,
            PlayMode = request.PlayMode,
            IsTokkunPlayResult = request.PlayMode == (uint)PlayMode.Tokkun || request.AryTokkunstageInfo is not null,
            TokkunTutorialFlg = request.ShouldSerializeTokkunTutorialFlg() ? request.TokkunTutorialFlg : null,
            TokkunStageData = MapTokkunStageData(request.AryTokkunstageInfo),
            AreaCode = request.AreaCode,
            Reserved = request.Reserved ?? [],
            LowerlimitAge = request.ShouldSerializeLowerlimitAge() ? request.LowerlimitAge : null,
            UpperlimitAge = request.ShouldSerializeUpperlimitAge() ? request.UpperlimitAge : null,
            AgeScore = request.ShouldSerializeAgeScore() ? request.AgeScore : null,
            EstimationCount = request.ShouldSerializeEstimationCount() ? request.EstimationCount : null,
            DanResult = request.ShouldSerializeDanResult() ? request.DanResult : 0,
            AryCollaboInfo = request.AryCollaboInfoes.Select(MapCollabo).ToList(),
            TournamentMode = request.ShouldSerializeTournamentMode() ? request.TournamentMode : 0,
            Accesstoken = request.ShouldSerializeAccesstoken() ? request.Accesstoken : string.Empty,
            ContentInfo = request.ContentInfo ?? [],
            DifficultyPlayedCourse = request.DifficultyPlayedCourse,
            DifficultyPlayedStar = request.DifficultyPlayedStar,
            HasDifficultyPlayedCourse = request.ShouldSerializeDifficultyPlayedCourse(),
            HasDifficultyPlayedStar = request.ShouldSerializeDifficultyPlayedStar(),
            WaiwaiTutorialFlg = request.ShouldSerializeWaiwaiTutorialFlg() ? request.WaiwaiTutorialFlg : null
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
            HitCnt = stage.ShouldSerializeHitCnt() ? stage.HitCnt : 0,
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
            PlayDan = stage.PlayDan == 0 ? null : stage.PlayDan,
            SoulGauge = stage.ShouldSerializeSoulGauge() ? stage.SoulGauge : null,
            StageMode = stage.StageMode,
            SelectedFolderId = stage.SelectedFolderId,
            WaiwaiResult = stage.WaiwaiResult,
            WaiwaiGauge = stage.WaiwaiGauge
        };
    }

    private static CommonPlayResultData.ResultcompeData MapCompe(PlayResultRequest.StageData.ResultcompeData data)
    {
        return new CommonPlayResultData.ResultcompeData
        {
            CompeId = data.CompeId,
            TrackNo = data.TrackNo
        };
    }

    private static CommonPlayResultData.CostumeData MapCostume(PlayResultRequest.CostumeData? costume)
    {
        return costume is null
            ? new CommonPlayResultData.CostumeData()
            : new CommonPlayResultData.CostumeData
            {
                Costume1 = costume.Costume1,
                Costume2 = costume.Costume2,
                Costume3 = costume.Costume3,
                Costume4 = costume.Costume4,
                Costume5 = costume.Costume5
            };
    }

    private static CommonPlayResultData.CollaboData MapCollabo(PlayResultRequest.CollaboData collabo)
    {
        return new CommonPlayResultData.CollaboData
        {
            CollaboSelect = collabo.CollaboSelect,
            CollaboId = collabo.ShouldSerializeCollaboId() ? collabo.CollaboId : null,
            CollaboResult = collabo.ShouldSerializeCollaboResult() ? collabo.CollaboResult : null
        };
    }

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
