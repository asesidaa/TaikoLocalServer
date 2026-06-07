namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers;

public static class TournamentMappers
{
    public static TournamentcheckResponse Map(CommonTournamentCheckResponse common)
    {
        var response = new TournamentcheckResponse
        {
            Result = common.Result,
            RareRate = common.RareRate,
            SongHashVer = common.SongHashVer
        };

        response.AryGachaSongDatas.AddRange(common.AryGachaSongData.Select(MapGacha));
        response.AryGachaToneDatas.AddRange(common.AryGachaToneData.Select(MapGacha));
        response.AryGachaCostume1Datas.AddRange(common.AryGachaCostume1Data.Select(MapGacha));
        response.AryGachaCostume2Datas.AddRange(common.AryGachaCostume2Data.Select(MapGacha));
        response.AryGachaCostume3Datas.AddRange(common.AryGachaCostume3Data.Select(MapGacha));
        response.AryGachaCostume4Datas.AddRange(common.AryGachaCostume4Data.Select(MapGacha));
        response.AryGachaCostume5Datas.AddRange(common.AryGachaCostume5Data.Select(MapGacha));
        response.AryGachaTitleDatas.AddRange(common.AryGachaTitleData.Select(MapGacha));

        return response;
    }

    private static TournamentcheckResponse.GachainfoData MapGacha(
        CommonTournamentCheckResponse.GachainfoData common)
        => new()
        {
            NormalGachaFlg = common.NormalGachaFlg,
            RareGachaFlg = common.RareGachaFlg
        };
}
