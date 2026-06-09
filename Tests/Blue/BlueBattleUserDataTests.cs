using TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;
using TaikoLocalServer.Adapters.GameProtocol.Blue.Wire;
using TaikoLocalServer.Application.Catalog.Blue;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueBattleUserDataTests
{
    [Fact]
    public async Task Handle_NewUser_ReturnsIdaSafeStarterState()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        await AddUserAsync(fixture.Context, 501);
        var handler = new GetBattleUserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<GetBattleUserDataQueryHandler>.Instance);

        var common = await handler.Handle(new GetBattleUserDataQuery(501), CancellationToken.None);
        var wire = BattleUserDataMappers.Map(common);

        AssertSafeDefaultBattleUserData(wire);
    }

    [Fact]
    public async Task Handle_NewUserWithBattleCatalog_DoesNotDeriveBattleUserDataRuntimeState()
    {
        var battleCatalog = new BlueBattleCatalog
        {
            IsRawDataAvailable = true,
            EnablesBattleAdvertisement = true,
            BattleNpcs =
            [
                new() { NpcId = 7, StartExp = 12, InitialDpn = 30 },
                new() { NpcId = 9, StartExp = 99, InitialDpn = 40 }
            ],
            BattleNpcIds = [7, 9],
            ReleaseBattleStageIds = [1, 2],
            ReleaseBattleSpecialIds = [1, 2, 3, 7],
            Files = []
        };
        var blueCatalog = new BlueHandlerFixture.TestBlueCatalog(battleCatalog: battleCatalog);
        await using var fixture = await BlueHandlerFixture.CreateAsync(blueCatalog);
        await AddUserAsync(fixture.Context, 504);
        var handler = new GetBattleUserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<GetBattleUserDataQueryHandler>.Instance);

        var common = await handler.Handle(new GetBattleUserDataQuery(504), CancellationToken.None);
        var wire = BattleUserDataMappers.Map(common);

        AssertSafeDefaultBattleUserData(wire);
    }

    [Fact]
    public async Task Handle_PersistedBattleState_ReadsBackClientReportedProgress()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        await AddUserAsync(fixture.Context, 505);
        var now = new DateTime(2026, 5, 31, 15, 0, 0, DateTimeKind.Utc);
        fixture.Context.BlueBattleUserStates.Add(new BlueBattleUserState
        {
            Baid = 505,
            ReleaseInfoFlg = BlueProtocolBytes.CreateFixedBitset([1], BlueProtocolBytes.BattleInfoFlagBytes),
            ReleaseBattleStageFlg = new byte[BlueProtocolBytes.BattleStageFlagBytes],
            LastBattleStageId = 1,
            LastBossLife = 0,
            LastNpcId = 0,
            AssignStageId = 1,
            CreatedAt = now,
            UpdatedAt = now
        });
        fixture.Context.BlueBattleNpcStates.Add(new BlueBattleNpcState
        {
            Baid = 505,
            NpcId = 0,
            TotalExp = 175,
            MaxDpn = 34,
            NpcCostumeId = 0,
            NpcCostumeFlg = [1, 0, 0, 0],
            SelectedSpecialId1 = 1,
            SelectedSpecialId2 = 1,
            SelectedSpecialId3 = 1,
            ReleaseSpecialFlg = BlueProtocolBytes.CreateFixedBitset([1], BlueProtocolBytes.BattleSpecialFlagBytes),
            CreatedAt = now,
            UpdatedAt = now
        });
        fixture.Context.BlueBattleTokenStates.Add(new BlueBattleTokenState
        {
            Baid = 505,
            TokenId = 1,
            TokenValue = 9,
            CreatedAt = now,
            UpdatedAt = now
        });
        await fixture.Context.SaveChangesAsync();
        var handler = new GetBattleUserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<GetBattleUserDataQueryHandler>.Instance);

        var common = await handler.Handle(new GetBattleUserDataQuery(505), CancellationToken.None);
        var wire = BattleUserDataMappers.Map(common);

        Assert.Equal(1u, wire.Result);
        Assert.True(wire.ShouldSerializeReleaseInfoFlg());
        Assert.True(BitIsSet(wire.ReleaseInfoFlg, 1));
        Assert.True(wire.ShouldSerializeReleaseBattleStageFlg());
        Assert.True(BitIsSet(wire.ReleaseBattleStageFlg, 1));
        Assert.True(wire.ShouldSerializeLastBattleStageId());
        Assert.Equal(1u, wire.LastBattleStageId);
        Assert.True(wire.ShouldSerializeLastBossLife());
        Assert.Equal(0u, wire.LastBossLife);
        Assert.True(wire.ShouldSerializeLastNpcId());
        Assert.Equal(0u, wire.LastNpcId);
        Assert.True(wire.ShouldSerializeAssignStageId());
        Assert.Equal(1u, wire.AssignStageId);

        var npc = Assert.Single(wire.NpcDatas);
        Assert.Equal(0u, npc.NpcId);
        Assert.Equal("175", npc.TotalExp);
        Assert.Equal(34u, npc.MaxDpn);
        Assert.Equal(0u, npc.NpcCostumeId);
        Assert.True(BitIsSet(npc.NpcCostumeFlg, 0));
        Assert.Equal(1u, npc.LastSelectSpecial1);
        Assert.Equal(1u, npc.LastSelectSpecial2);
        Assert.Equal(1u, npc.LastSelectSpecial3);
        Assert.True(npc.ShouldSerializeReleaseSpecialFlg());
        Assert.True(BitIsSet(npc.ReleaseSpecialFlg, 1));
        Assert.True(BitIsSet(npc.ReleaseSpecialFlg, BlueProtocolBytes.BattleNpcSpecialRowGateId));

        // The battle-intro selector (EBOOT sub_EEF48) does an unguarded flat_map::at(0) on the token
        // map, so a token_id 0 row must always be present or the game crashes entering battle. The
        // client-reported normal token (id 1, value 9) is echoed at its real id so song-select reflects it.
        Assert.Equal(2, wire.AryTokenDatas.Count);
        var introToken = wire.AryTokenDatas.Single(t => t.TokenId == 0);
        Assert.Equal(0u, introToken.TokenValue);
        var normalToken = wire.AryTokenDatas.Single(t => t.TokenId == 1);
        Assert.Equal(9u, normalToken.TokenValue);
    }

    [Fact]
    public void Map_OnlySetsOptionalWireFieldsAndRowsPresentInCommonDto()
    {
        var common = new CommonBattleUserDataResponse
        {
            Result = 1,
            LastNpcId = 22,
            NpcDatas =
            [
                new CommonBattleUserDataResponse.BattleUserNpcData
                {
                    NpcId = 23,
                    TotalExp = "24",
                    MaxDpn = 25,
                    NpcCostumeId = 26,
                    NpcCostumeFlg = [27, 28, 29, 30],
                    LastSelectSpecial1 = 31,
                    LastSelectSpecial2 = 32,
                    LastSelectSpecial3 = 33,
                    ReleaseSpecialFlg = [34, 35]
                }
            ],
            AryTokenDatas =
            [
                new CommonBattleUserDataResponse.BattleUserTokenData
                {
                    TokenId = 36,
                    TokenValue = 37
                }
            ]
        };

        var wire = BattleUserDataMappers.Map(common);

        Assert.Equal(1u, wire.Result);
        Assert.False(wire.ShouldSerializeReleaseInfoFlg());
        Assert.False(wire.ShouldSerializeReleaseBattleStageFlg());
        Assert.False(wire.ShouldSerializeLastBattleStageId());
        Assert.False(wire.ShouldSerializeLastBossLife());
        Assert.True(wire.ShouldSerializeLastNpcId());
        Assert.Equal(22u, wire.LastNpcId);
        Assert.False(wire.ShouldSerializeAssignStageId());

        var npc = Assert.Single(wire.NpcDatas);
        Assert.Equal(23u, npc.NpcId);
        Assert.Equal("24", npc.TotalExp);
        Assert.Equal(25u, npc.MaxDpn);
        Assert.Equal(26u, npc.NpcCostumeId);
        Assert.Equal([27, 28, 29, 30], npc.NpcCostumeFlg);
        Assert.Equal(31u, npc.LastSelectSpecial1);
        Assert.Equal(32u, npc.LastSelectSpecial2);
        Assert.Equal(33u, npc.LastSelectSpecial3);
        Assert.True(npc.ShouldSerializeReleaseSpecialFlg());
        Assert.Equal([34, 35], npc.ReleaseSpecialFlg);

        var token = Assert.Single(wire.AryTokenDatas);
        Assert.Equal(36u, token.TokenId);
        Assert.Equal(37u, token.TokenValue);
    }

    private static void AssertSafeDefaultBattleUserData(BattleUserDataResponse wire)
    {
        Assert.Equal(1u, wire.Result);
        // release_info_flg is the "clip already seen" suppression mask (EBOOT sub_EEF48 plays a clip
        // only while its clip-id bit is CLEAR). A brand-new user has seen nothing, so every bit must
        // be clear — in particular clip-id 1 (intro/01 + JINGLE_BTLPROLOG, the battle tutorial).
        Assert.True(wire.ShouldSerializeReleaseInfoFlg());
        Assert.All(wire.ReleaseInfoFlg, b => Assert.Equal(0, b));
        // release_battle_stage_flg is an unlock mask: stage 1 must be unlocked so its substage intro resolves.
        Assert.True(wire.ShouldSerializeReleaseBattleStageFlg());
        Assert.True(BitIsSet(wire.ReleaseBattleStageFlg, 1));
        Assert.True(wire.ShouldSerializeLastBattleStageId());
        Assert.Equal(BlueProtocolBytes.BattleDefaultStageId, wire.LastBattleStageId);
        Assert.True(wire.ShouldSerializeLastBossLife());
        Assert.Equal(0u, wire.LastBossLife);
        Assert.True(wire.ShouldSerializeLastNpcId());
        Assert.Equal(0u, wire.LastNpcId);
        Assert.True(wire.ShouldSerializeAssignStageId());
        Assert.Equal(1u, wire.AssignStageId);

        var npc = Assert.Single(wire.NpcDatas);
        Assert.Equal(0u, npc.NpcId);
        Assert.Equal("0", npc.TotalExp);
        Assert.Equal(0u, npc.MaxDpn);
        Assert.Equal(0u, npc.NpcCostumeId);
        Assert.True(BitIsSet(npc.NpcCostumeFlg, 0));
        Assert.Equal(BlueProtocolBytes.BattleDefaultSpecialId, npc.LastSelectSpecial1);
        Assert.Equal(0u, npc.LastSelectSpecial2);
        Assert.Equal(0u, npc.LastSelectSpecial3);
        Assert.True(npc.ShouldSerializeReleaseSpecialFlg());
        Assert.Equal(
            BlueProtocolBytes.CreateBattleSpecialBitset([BlueProtocolBytes.BattleDefaultSpecialId]),
            npc.ReleaseSpecialFlg);

        var token = Assert.Single(wire.AryTokenDatas);
        Assert.Equal(0u, token.TokenId);
        Assert.Equal(0u, token.TokenValue);
    }

    private static async Task AddUserAsync(TaikoDbContext context, uint baid)
    {
        context.UserData.Add(new UserDatum
        {
            Baid = baid,
            MyDonName = $"Baid {baid}"
        });
        await context.SaveChangesAsync();
    }

    private static bool BitIsSet(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;

}
