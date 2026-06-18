using TaikoLocalServer.Tests.Blue;
using TaikoLocalServer.Tests.Green;
using TaikoLocalServer.Tests.Red;
using TaikoLocalServer.Tests.Yellow;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class RecentSongLimitTests
{
    private const int OfficialRecentSongLimit = 5;

    [Fact]
    public async Task UserData_Blue_ReturnsAtMostFiveRecentSongs()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        fixture.Context.BlueRecentSongs.AddRange(CreateRecentRows<BlueRecentSongs>());
        await fixture.Context.SaveChangesAsync();

        var response = await CreateAc15Handler(fixture.Context, fixture.Catalog)
            .Handle(new Ac15UserDataQuery(1, GameEra.Blue), CancellationToken.None);

        Assert.Equal([107u, 106u, 105u, 104u, 103u], response.SongLists.AryRecentSongNoes);
        Assert.Equal(OfficialRecentSongLimit, response.SongLists.AryRecentSongNoes.Length);
    }

    [Fact]
    public async Task UserData_Green_ReturnsAtMostFiveRecentSongs()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        fixture.Context.GreenRecentSongs.AddRange(CreateRecentRows<GreenRecentSongs>());
        await fixture.Context.SaveChangesAsync();

        var response = await CreateAc15Handler(fixture.Context, fixture.Catalog)
            .Handle(new Ac15UserDataQuery(1, GameEra.Green), CancellationToken.None);

        Assert.Equal([107u, 106u, 105u, 104u, 103u], response.SongLists.AryRecentSongNoes);
        Assert.Equal(OfficialRecentSongLimit, response.SongLists.AryRecentSongNoes.Length);
    }

    [Fact]
    public async Task UserData_Yellow_ReturnsAtMostFiveRecentSongs()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataYellow.Add(UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(1));
        fixture.Context.YellowRecentSongs.AddRange(CreateRecentRows<YellowRecentSongs>());
        await fixture.Context.SaveChangesAsync();

        var response = await CreateAc15Handler(fixture.Context, fixture.Catalog)
            .Handle(new Ac15UserDataQuery(1, GameEra.Yellow), CancellationToken.None);

        Assert.Equal([107u, 106u, 105u, 104u, 103u], response.SongLists.AryRecentSongNoes);
        Assert.Equal(OfficialRecentSongLimit, response.SongLists.AryRecentSongNoes.Length);
    }

    [Fact]
    public async Task UserData_Red_ReturnsAtMostFiveRecentSongs()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataRed.Add(UserSaveDataRedExtensions.CreateDefaultRedSaveData(1));
        fixture.Context.RedRecentSongs.AddRange(CreateRecentRows<RedRecentSongs>());
        await fixture.Context.SaveChangesAsync();

        var response = await CreateAc15Handler(fixture.Context, fixture.Catalog)
            .Handle(new Ac15UserDataQuery(1, GameEra.Red), CancellationToken.None);

        Assert.Equal([107u, 106u, 105u, 104u, 103u], response.SongLists.AryRecentSongNoes);
        Assert.Equal(OfficialRecentSongLimit, response.SongLists.AryRecentSongNoes.Length);
    }

    private static UserDataQueryHandler CreateAc15Handler(TaikoDbContext context, IGameDataCatalog catalog)
        => new(
            context,
            catalog,
            NullLogger<UserDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

    private static IEnumerable<TRecent> CreateRecentRows<TRecent>()
        where TRecent : class, IAc15RecentSong, new()
        => Enumerable.Range(101, 7)
            .Select(song => new TRecent
            {
                Baid = 1,
                SongNo = (uint)song,
                LastPlayed = new DateTime(2026, 6, song - 100, 12, 0, 0)
            });
}
