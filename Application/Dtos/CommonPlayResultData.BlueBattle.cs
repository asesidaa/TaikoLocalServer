// ReSharper disable InconsistentNaming
namespace TaikoLocalServer.Application.Dtos;

public partial class CommonPlayResultData
{
    public bool IsBattlePlayResult { get; set; }

    public BattleReleaseDataDto? BattleReleaseData { get; set; }

    public partial class StageData
    {
        public BattleStageData? BattleStageData { get; set; }
    }

    public class BattleStageData
    {
        public uint SupportLv { get; set; }

        public uint BattleStageId { get; set; }

        public BattleNpcData? NpcData { get; set; }

        public uint KillCnt { get; set; }

        public uint BossLife { get; set; }

        public uint TotalDamage { get; set; }

        public uint CriticalCnt { get; set; }

        public uint SpecialMoveCnt { get; set; }
    }

    public class BattleNpcData
    {
        public uint NpcId { get; set; }

        public string AcquiredExp { get; set; } = string.Empty;

        public string TotalExp { get; set; } = string.Empty;

        public uint Dpn { get; set; }

        public uint NpcCostumeId { get; set; }

        public uint SpecialId1 { get; set; }

        public uint SpecialId2 { get; set; }

        public uint SpecialId3 { get; set; }

        public uint BondsLv { get; set; }
    }

    public class BattleReleaseDataDto
    {
        public List<uint> ReleaseInfoIds { get; set; } = [];

        public List<uint> ReleaseBattleStageIds { get; set; } = [];

        public List<uint> ReleaseNpcIds { get; set; } = [];

        public List<uint> ReleaseNpcCostumeIds { get; set; } = [];

        public List<uint> ReleaseNpcSpecialIds { get; set; } = [];

        public List<BattleTokenData> BattleTokenData { get; set; } = [];

        public uint AssignNextStageId { get; set; }
    }

    public class BattleTokenData
    {
        public uint TokenId { get; set; }

        public uint TokenValue { get; set; }
    }
}
