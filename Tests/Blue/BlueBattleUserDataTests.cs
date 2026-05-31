using TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;
using TaikoLocalServer.Adapters.GameProtocol.Blue.Wire;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueBattleUserDataTests
{
    [Fact]
    public async Task Handle_NewUser_ReturnsResultAndLeavesUnresolvedFieldsUnemitted()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        await AddUserAsync(fixture.Context, 501);
        var handler = new GetBattleUserDataQueryHandler(
            fixture.Context,
            NullLogger<GetBattleUserDataQueryHandler>.Instance);

        var common = await handler.Handle(new GetBattleUserDataQuery(501), CancellationToken.None);
        var wire = BattleUserDataMappers.Map(common);

        Assert.Equal(1u, common.Result);
        Assert.Equal(1u, wire.Result);
        Assert.False(wire.ShouldSerializeReleaseInfoFlg());
        Assert.False(wire.ShouldSerializeReleaseBattleStageFlg());
        Assert.False(wire.ShouldSerializeLastBattleStageId());
        Assert.False(wire.ShouldSerializeLastBossLife());
        Assert.False(wire.ShouldSerializeLastNpcId());
        Assert.False(wire.ShouldSerializeAssignStageId());
        Assert.Empty(wire.NpcDatas);
        Assert.Empty(wire.AryTokenDatas);
    }

    [Fact]
    public async Task Handle_PersistedApprovedState_EmitsOnlyPersistedScalarsAndCompleteTokenRows()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        await AddUserAsync(fixture.Context, 502);
        var now = new DateTime(2026, 5, 31, 13, 0, 0, DateTimeKind.Utc);
        fixture.Context.BlueBattleUserStates.Add(new BlueBattleUserState
        {
            Baid = 502,
            ReleaseInfoFlg = [1, 2, 3],
            ReleaseBattleStageFlg = [4, 5],
            LastBattleStageId = 6,
            LastBossLife = 7,
            AssignStageId = 8,
            CreatedAt = now,
            UpdatedAt = now
        });
        fixture.Context.BlueBattleNpcStates.Add(new BlueBattleNpcState
        {
            Baid = 502,
            NpcId = 9,
            TotalExp = 10,
            CreatedAt = now,
            UpdatedAt = now
        });
        fixture.Context.BlueBattleTokenStates.Add(new BlueBattleTokenState
        {
            Baid = 502,
            TokenId = 11,
            TokenValue = 12,
            CreatedAt = now,
            UpdatedAt = now
        });
        await fixture.Context.SaveChangesAsync();
        var handler = new GetBattleUserDataQueryHandler(
            fixture.Context,
            NullLogger<GetBattleUserDataQueryHandler>.Instance);

        var common = await handler.Handle(new GetBattleUserDataQuery(502), CancellationToken.None);
        var wire = BattleUserDataMappers.Map(common);

        Assert.Equal([1, 2, 3], wire.ReleaseInfoFlg);
        Assert.Equal([4, 5], wire.ReleaseBattleStageFlg);
        Assert.Equal(6u, wire.LastBattleStageId);
        Assert.Equal(7u, wire.LastBossLife);
        Assert.Equal(8u, wire.AssignStageId);
        Assert.False(wire.ShouldSerializeLastNpcId());
        Assert.Empty(wire.NpcDatas);
        var token = Assert.Single(wire.AryTokenDatas);
        Assert.Equal(11u, token.TokenId);
        Assert.Equal(12u, token.TokenValue);
    }

    [Fact]
    public async Task Handle_PersistedCompleteNpcRows_EmitsNpcDatasWithSelectedSpecials()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        await AddUserAsync(fixture.Context, 503);
        var now = new DateTime(2026, 5, 31, 14, 0, 0, DateTimeKind.Utc);
        fixture.Context.BlueBattleNpcStates.Add(new BlueBattleNpcState
        {
            Baid = 503,
            NpcId = 9,
            TotalExp = 888,
            MaxDpn = 456,
            NpcCostumeId = 30,
            NpcCostumeFlg = [0, 0, 0, 64],
            SelectedSpecialId1 = 21,
            SelectedSpecialId2 = 22,
            SelectedSpecialId3 = 23,
            ReleaseSpecialFlg = [0, 0, 224],
            CreatedAt = now,
            UpdatedAt = now
        });
        fixture.Context.BlueBattleNpcStates.Add(new BlueBattleNpcState
        {
            Baid = 503,
            NpcId = 10,
            SelectedSpecialId1 = 24,
            SelectedSpecialId2 = 25,
            SelectedSpecialId3 = 26,
            CreatedAt = now,
            UpdatedAt = now
        });
        await fixture.Context.SaveChangesAsync();
        var handler = new GetBattleUserDataQueryHandler(
            fixture.Context,
            NullLogger<GetBattleUserDataQueryHandler>.Instance);

        var common = await handler.Handle(new GetBattleUserDataQuery(503), CancellationToken.None);
        var wire = BattleUserDataMappers.Map(common);

        var npc = Assert.Single(wire.NpcDatas);
        Assert.Equal(9u, npc.NpcId);
        Assert.Equal("888", npc.TotalExp);
        Assert.Equal(456u, npc.MaxDpn);
        Assert.Equal(30u, npc.NpcCostumeId);
        Assert.Equal([0, 0, 0, 64], npc.NpcCostumeFlg);
        Assert.Equal(21u, npc.LastSelectSpecial1);
        Assert.Equal(22u, npc.LastSelectSpecial2);
        Assert.Equal(23u, npc.LastSelectSpecial3);
        Assert.True(npc.ShouldSerializeReleaseSpecialFlg());
        Assert.Equal([0, 0, 224, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0], npc.ReleaseSpecialFlg);
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

    [Fact]
    public void BattleUserDataController_UsesMediatorQueryAndMapperInsteadOfLocalSuccessConstruction()
    {
        var root = FindRepoRoot();
        var source = File.ReadAllText(Path.Combine(
            root,
            "Adapters.GameProtocol.Blue",
            "Controllers",
            "BattleUserDataController.cs"));

        Assert.Contains("Mediator.Send(new GetBattleUserDataQuery", source, StringComparison.Ordinal);
        Assert.Contains("BattleUserDataMappers.Map", source, StringComparison.Ordinal);
        Assert.DoesNotContain("new BattleUserDataResponse { Result = 1 }", source, StringComparison.Ordinal);
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

    private static string FindRepoRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "TaikoLocalServer.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("Could not find TaikoLocalServer.slnx.");
    }
}
