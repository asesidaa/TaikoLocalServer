using System.Collections.Immutable;
using TaikoLocalServer.Application.ServerData;
using TaikoLocalServer.Tests.Green;

namespace TaikoLocalServer.Tests.Nijiiro;

// Dani Dojo results are kept per dan set: the one the cabinet serves (its own, else the server default). A cabinet reads
// back only its set's results, so a new set (a new year's courses) starts fresh, and the old set's results stay.
public sealed class DanSetResultsTests
{
    internal static IGameDataCatalog Catalog() =>
        new FileGameDataCatalog([new EventFolderLookupSharedTests.TestNijiiroCatalog(ImmutableDictionary<uint, EventFolderData>.Empty)]);

    private sealed class Cabinets(params Cabinet[] cabinets) : ICabinetRegistry
    {
        public bool Restricted => false;
        public Task InitializeAsync() => Task.CompletedTask;
        public bool Seen(string chassisId, string shopId, string version) => true;
        public void Located(string chassisId, string locationName) { }
        public IReadOnlyList<Cabinet> List() => cabinets;
        public Cabinet? Find(string chassisId) => cabinets.FirstOrDefault(c => c.ChassisId == chassisId);
        public Task UpdateAsync(string chassisId, Action<Cabinet> change, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<bool> RemoveAsync(string chassisId, CancellationToken cancellationToken) => Task.FromResult(false);
    }

    private static UpdatePlayResultCommand Run(string chassis, DanClearState result, uint soul) => new(1, GameEra.Nijiiro, new CommonPlayResultData
    {
        Baid = 1, PlayMode = (uint)PlayMode.DanMode, DanId = 13, DanResult = (uint)result, SoulGaugeTotal = soul,
        AryStageInfoes = [new CommonPlayResultData.StageData { PlayScore = 900_000, OptionFlg = [0] }],
    }, chassis);

    [Fact]
    public async Task EachCabinetsSet_KeepsAndReadsBackItsOwnResults()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        await fixture.Context.SaveChangesAsync();
        var catalog = Catalog();
        var cabinets = new Cabinets(new Cabinet { ChassisId = "NEW", OwnSettings = true, DanSet = "y2026" });
        var play = new UpdatePlayResultCommandHandler(fixture.Context, catalog, NullLogger<UpdatePlayResultCommandHandler>.Instance, cabinets: cabinets);
        var read = new GetDanScoreQueryHandler(NullLogger<GetDanScoreQueryHandler>.Instance, fixture.Context, catalog, cabinets);

        await play.Handle(Run("OLD", DanClearState.GoldNormalClear, 100), CancellationToken.None);   // default set
        await play.Handle(Run("NEW", DanClearState.NotClear, 40), CancellationToken.None);           // y2026
        fixture.Context.ChangeTracker.Clear();

        var rows = await fixture.Context.DanScoreDataNijiiro.OrderBy(d => d.DanSet).ToListAsync();
        Assert.Equal(["dan_data", "y2026"], rows.Select(r => r.DanSet));
        Assert.Equal(DanClearState.GoldNormalClear, rows[0].ClearState);   // the new set's fail didn't touch it

        async Task<uint> SoulOn(string chassis) =>
            (await read.Handle(new GetDanScoreQuery(1, GameEra.Nijiiro, (uint)DanType.Normal, [13], chassis), CancellationToken.None)).AryDanScoreDatas.Single().SoulGaugeTotal;
        Assert.Equal(40u, await SoulOn("NEW"));
        Assert.Equal(100u, await SoulOn("OLD"));
    }
}
