using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf;
using TaikoLocalServer.Adapters.GameProtocol.White.Controllers;
using TaikoLocalServer.Adapters.GameProtocol.White.Wire;
using TaikoLocalServer.Application;

namespace TaikoLocalServer.Tests.White;

public sealed class WhitePersonIdCompatibilityTests
{
    [Fact]
    public async Task BaidCheck_FinalExistingUserSerializesPersonIdForTokkunCompatibility()
    {
        await using var fixture = await WhiteHandlerFixture.CreateAsync();
        fixture.Context.Cards.Add(new Card { AccessCode = "12345678901234567890", Baid = 1 });
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "WHITE" });
        fixture.Context.UserSaveDataWhite.Add(UserSaveDataWhiteExtensions.CreateDefaultWhiteSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var response = await InvokeAsync(
            new BaidController(),
            fixture,
            controller => controller.BaidCheck(new BAIDRequest { AccessCode = "12345678901234567890" }),
            value => Assert.IsType<BAIDResponse>(value));

        Assert.Equal("1", response.Personid);
        Assert.True(response.ShouldSerializePersonid());
        Assert.Contains(32, ReadFieldNumbers(Serialize(response)));
    }

    [Fact]
    public async Task MyDonEntry_FinalSerializesPersonIdForTokkunCompatibility()
    {
        await using var fixture = await WhiteHandlerFixture.CreateAsync();

        var response = await InvokeAsync(
            new MyDonEntryController(),
            fixture,
            controller => controller.MydonEntry(new MydonEntryRequest
            {
                AccessCode = "12345678901234567891",
                MydonName = "WHITE"
            }),
            value => Assert.IsType<MydonEntryResponse>(value));

        Assert.Equal("1", response.Personid);
        Assert.True(response.ShouldSerializePersonid());
        Assert.Contains(15, ReadFieldNumbers(Serialize(response)));
    }

    private static async Task<TResponse> InvokeAsync<TController, TResponse>(
        TController controller,
        WhiteHandlerFixture fixture,
        Func<TController, Task<IActionResult>> action,
        Func<object?, TResponse> assertResponse)
        where TController : ControllerBase
    {
        await using var services = CreateServices(fixture);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { RequestServices = services }
        };

        var ok = Assert.IsType<OkObjectResult>(await action(controller));
        return assertResponse(ok.Value);
    }

    private static ServiceProvider CreateServices(WhiteHandlerFixture fixture)
        => new ServiceCollection()
            .AddLogging()
            .AddApplication()
            .AddScoped<ITaikoDbContext>(_ => fixture.Context)
            .AddScoped<IGameDataCatalog>(_ => fixture.Catalog)
            .BuildServiceProvider();

    private static byte[] Serialize<T>(T value)
    {
        using var stream = new MemoryStream();
        Serializer.Serialize(stream, value);
        return stream.ToArray();
    }

    private static HashSet<int> ReadFieldNumbers(byte[] protobuf)
    {
        var fields = new HashSet<int>();
        var offset = 0;
        while (offset < protobuf.Length)
        {
            var tag = ReadVarint(protobuf, ref offset);
            fields.Add((int)(tag >> 3));
            var wireType = (int)(tag & 7);
            switch (wireType)
            {
                case 0:
                    _ = ReadVarint(protobuf, ref offset);
                    break;
                case 1:
                    offset += 8;
                    break;
                case 2:
                    var length = (int)ReadVarint(protobuf, ref offset);
                    offset += length;
                    break;
                case 5:
                    offset += 4;
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported protobuf wire type {wireType}.");
            }
        }

        return fields;
    }

    private static ulong ReadVarint(byte[] buffer, ref int offset)
    {
        ulong result = 0;
        var shift = 0;
        while (offset < buffer.Length)
        {
            var b = buffer[offset++];
            result |= (ulong)(b & 0x7F) << shift;
            if ((b & 0x80) == 0)
            {
                return result;
            }

            shift += 7;
        }

        throw new InvalidOperationException("Unterminated protobuf varint.");
    }
}
