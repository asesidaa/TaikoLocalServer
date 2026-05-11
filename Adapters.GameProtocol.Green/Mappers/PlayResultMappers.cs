using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

[Mapper]
public static partial class PlayResultMappers
{
    public static CommonPlayResultData Map(PlayResultDataRequest request)
    {
        // TODO iter 2: map the full Green play-result payload.
        return new CommonPlayResultData
        {
            Baid = request.Baid,
            ChassisId = request.ChassisId,
            ShopId = request.ShopId,
            PlayDatetime = request.PlayDatetime,
            IsRight = request.IsRight,
            CardType = request.CardType,
            IsTwoPlayers = request.IsTwoPlayers,
            GetDonmedal = request.GetDonmedal,
            GetKatsumedal = request.GetKatsumedal,
            BonusDailyFlg = request.BonusDailyFlg,
            BonusWeeklyFlg = request.BonusWeeklyFlg,
            BonusMonthlyFlg = request.BonusMonthlyFlg,
            GenderType = request.GenderType,
            PlayerAge = request.PlayerAge,
            PlayMode = request.PlayMode,
            AreaCode = request.AreaCode,
            Reserved = request.Reserved ?? [],
            Accesstoken = request.Accesstoken,
            ContentInfo = request.ContentInfo ?? []
        };
    }

    public static PlayResultResponse Map(uint result)
    {
        return new PlayResultResponse { Result = result };
    }
}
