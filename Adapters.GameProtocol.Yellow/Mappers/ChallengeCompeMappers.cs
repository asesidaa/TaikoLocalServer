using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers;

[Mapper]
public static partial class ChallengeCompeMappers
{
    [MapProperty(nameof(CommonChallengeCompeResponse.AryChallengeStat), nameof(ChallengeCompeResponse.AryChallengeStats))]
    [MapProperty(nameof(CommonChallengeCompeResponse.AryUserCompeStat), nameof(ChallengeCompeResponse.AryUserCompeStats))]
    [MapProperty(nameof(CommonChallengeCompeResponse.AryBngCompeStat), nameof(ChallengeCompeResponse.AryBngCompeStats))]
    public static partial ChallengeCompeResponse Map(CommonChallengeCompeResponse common);

    [MapProperty(nameof(CommonChallengeCompeResponse.CompeData.AryTrackStat), nameof(ChallengeCompeResponse.CompeData.AryTrackStats))]
    private static partial ChallengeCompeResponse.CompeData MapCompe(
        CommonChallengeCompeResponse.CompeData common);
}
