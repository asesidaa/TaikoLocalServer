using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers;

[Mapper]
public static partial class TournamentMappers
{
    [MapProperty(nameof(CommonTournamentCheckResponse.AryGachaSongData), nameof(TournamentcheckResponse.AryGachaSongDatas))]
    [MapProperty(nameof(CommonTournamentCheckResponse.AryGachaToneData), nameof(TournamentcheckResponse.AryGachaToneDatas))]
    [MapProperty(nameof(CommonTournamentCheckResponse.AryGachaCostume1Data), nameof(TournamentcheckResponse.AryGachaCostume1Datas))]
    [MapProperty(nameof(CommonTournamentCheckResponse.AryGachaCostume2Data), nameof(TournamentcheckResponse.AryGachaCostume2Datas))]
    [MapProperty(nameof(CommonTournamentCheckResponse.AryGachaCostume3Data), nameof(TournamentcheckResponse.AryGachaCostume3Datas))]
    [MapProperty(nameof(CommonTournamentCheckResponse.AryGachaCostume4Data), nameof(TournamentcheckResponse.AryGachaCostume4Datas))]
    [MapProperty(nameof(CommonTournamentCheckResponse.AryGachaCostume5Data), nameof(TournamentcheckResponse.AryGachaCostume5Datas))]
    [MapProperty(nameof(CommonTournamentCheckResponse.AryGachaTitleData), nameof(TournamentcheckResponse.AryGachaTitleDatas))]
    public static partial TournamentcheckResponse Map(CommonTournamentCheckResponse common);
}
