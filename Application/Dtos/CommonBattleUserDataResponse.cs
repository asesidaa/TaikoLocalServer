namespace TaikoLocalServer.Application.Dtos;

public sealed class CommonBattleUserDataResponse
{
    public uint Result { get; set; }

    public byte[]? ReleaseInfoFlg { get; set; }

    public byte[]? ReleaseBattleStageFlg { get; set; }

    public uint? LastBattleStageId { get; set; }

    public uint? LastBossLife { get; set; }

    public uint? LastNpcId { get; set; }

    public List<BattleUserNpcData> NpcDatas { get; set; } = [];

    public List<BattleUserTokenData> AryTokenDatas { get; set; } = [];

    public uint? AssignStageId { get; set; }

    public sealed class BattleUserNpcData
    {
        public uint NpcId { get; set; }

        public string TotalExp { get; set; } = string.Empty;

        public uint MaxDpn { get; set; }

        public uint NpcCostumeId { get; set; }

        public byte[] NpcCostumeFlg { get; set; } = [];

        public uint LastSelectSpecial1 { get; set; }

        public uint LastSelectSpecial2 { get; set; }

        public uint LastSelectSpecial3 { get; set; }

        public byte[]? ReleaseSpecialFlg { get; set; }
    }

    public sealed class BattleUserTokenData
    {
        public uint TokenId { get; set; }

        public uint TokenValue { get; set; }
    }
}
