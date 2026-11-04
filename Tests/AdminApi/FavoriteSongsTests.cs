using System.Collections.Immutable;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Adapters.AdminApi.Controllers;
using TaikoLocalServer.Application.ServerData;
using TaikoLocalServer.Contracts.AdminApi.ServerData;
using TaikoLocalServer.Tests.Green;

namespace TaikoLocalServer.Tests.AdminApi;

public sealed class FavoriteSongsTests
{
    // The Web UI lists only music_order's songs, so a favourite set before a song left it is no longer in the catalog.
    [Fact]
    public async Task SetFavoriteSongs_KeepsSavedSongsTheWebUiNoLongerLists_ButRejectsNewUnknownOnes()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = new TaikoDbContext(new DbContextOptionsBuilder<TaikoDbContext>().UseSqlite(connection).Options);
        await context.Database.EnsureCreatedAsync();
        context.UserData.Add(new UserDatum { Baid = 1 });
        context.UserSaveDataNijiiro.Add(new UserSaveDataNijiiro { Baid = 1, FavoriteSongsArray = [10, 1487] });
        await context.SaveChangesAsync();
        var catalog = new FileGameDataCatalog([new EventFolderLookupSharedTests.TestNijiiroCatalog(
            ImmutableDictionary<uint, EventFolderData>.Empty,
            musicDetails: new Dictionary<uint, MusicDetail> { [10] = new() { SongId = 10 }, [20] = new() { SongId = 20 } })]);
        var controller = new FavoriteSongsController(context, catalog)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    RequestServices = new ServiceCollection().AddSingleton<IServerAccess>(new TestServerAccess()).BuildServiceProvider(),
                },
            },
        };

        Assert.IsType<NoContentResult>(await controller.SetFavoriteSongs(1, [1487, 20, 10]));
        Assert.IsType<BadRequestObjectResult>(await controller.SetFavoriteSongs(1, [1487, 20, 10, 999]));

        context.ChangeTracker.Clear();
        Assert.Equal([1487u, 20, 10], (await context.UserSaveDataNijiiro.SingleAsync()).FavoriteSongsArray);
    }
}
