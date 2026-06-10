using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;
using TaikoLocalServer.Adapters.GameProtocol.Blue.Wire;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueCrownsDataTests
{
    [Fact]
    public void BuildBlueCrownResponseBody_EmptyRowsProduceAllZeroBody()
    {
        var packed = BlueCrownResponseBuilder.BuildInflatedBody([], new BlueHandlerFixture.TestBlueCatalog());

        Assert.Equal(BlueProtocolBytes.CrownInflatedBytes, packed.Length);
        Assert.All(packed, value => Assert.Equal(0, value));
    }

    [Fact]
    public void BuildBlueCrownResponseBody_PacksSavedBestRowsAndIncludesShin()
    {
        var rows = new[]
        {
            new SongBestDatumBlue
            {
                Baid = 1,
                SongId = 101,
                Difficulty = Difficulty.Easy,
                IsShin = false,
                BestCrown = CrownType.Clear
            },
            new SongBestDatumBlue
            {
                Baid = 1,
                SongId = 101,
                Difficulty = Difficulty.Easy,
                IsShin = true,
                BestCrown = CrownType.Gold
            }
        };

        var packed = BlueCrownResponseBuilder.BuildInflatedBody(rows, new BlueHandlerFixture.TestBlueCatalog());

        var value = ReadTenBitValue(packed, 101);
        Assert.Equal(BlueProtocolBytes.BuildBlueCrownValue(
            Ac15CrownState.FullCombo,
            Ac15CrownState.None,
            Ac15CrownState.None,
            Ac15CrownState.None,
            Ac15CrownState.None), value);
    }

    [Fact]
    public async Task CrownsData_Blue_ReturnsGzipPackedCrownTable()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.SongBestDataBlue.Add(new SongBestDatumBlue
        {
            Baid = 1,
            SongId = 101,
            Difficulty = Difficulty.Easy,
            IsShin = false,
            BestScore = 1000,
            BestRate = 80,
            BestCrown = CrownType.Clear
        });
        await fixture.Context.SaveChangesAsync();
        using var provider = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();
        var controller = new CrownsDataController(fixture.Context, fixture.Catalog)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = provider }
            }
        };

        var result = await controller.CrownsData(new CrownsDataRequest
        {
            Baid = 1,
            ChassisId = "268410000000",
            ShopId = "JPN0JPN0123"
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<CrownsDataResponse>(ok.Value);
        Assert.Equal(1u, response.Result);
        Assert.Equal(456u, response.SongHashVer);
        using var input = new MemoryStream(response.HashCrownFlg);
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        gzip.CopyTo(output);
        Assert.Equal(BlueProtocolBytes.CrownInflatedBytes, output.ToArray().Length);
    }

    private static ushort ReadTenBitValue(byte[] packed, int songIndex)
    {
        ushort value = 0;
        var bitOffset = songIndex * 10;
        for (var bit = 0; bit < 10; bit++)
        {
            var absoluteBit = bitOffset + bit;
            if ((packed[absoluteBit >> 3] & (1 << (absoluteBit & 7))) != 0)
            {
                value |= (ushort)(1 << bit);
            }
        }

        return value;
    }
}
