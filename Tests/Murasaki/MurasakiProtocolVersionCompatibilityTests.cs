using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf;
using TaikoLocalServer.Adapters.GameProtocol.Murasaki.Mappers;
using TaikoLocalServer.Application.ServerData;
using FinalControllers = TaikoLocalServer.Adapters.GameProtocol.Murasaki.Controllers;
using FinalWire = TaikoLocalServer.Adapters.GameProtocol.Murasaki.Wire;
using LegacyWire = TaikoLocalServer.Adapters.GameProtocol.Murasaki.LegacyWire;

namespace TaikoLocalServer.Tests.Murasaki;

public sealed class MurasakiProtocolVersionCompatibilityTests
{
    [Fact]
    public void GetFolder_FinalSchemaSerializesRepeatedEventFolderRows()
    {
        var response = FolderDataMappers.Map(new CommonGetFolderResponse
        {
            Result = 1,
            AryEventfolderDatas =
            [
                new EventFolderData { FolderId = 7, SongNoes = [101, 102] },
                new EventFolderData { FolderId = 9, SongNoes = [201] }
            ]
        });

        var rows = response.AryEventfolderDatas.ToArray();
        Assert.Equal(1u, response.Result);
        Assert.Equal([7u, 9u], rows.Select(row => row.FolderId).ToArray());
        Assert.Equal([101u, 102u], rows[0].SongNoes);
        Assert.Equal([201u], rows[1].SongNoes);

        var fields = ReadTopLevelFieldNumbers(Serialize(response));
        Assert.Contains(2, fields);
        Assert.DoesNotContain(3, fields);
    }

    [Fact]
    public void GetFolder_CompatibilitySchemaPreservesScalarFolderShape()
    {
        var response = LegacyWire.LegacyFolderDataMappers.MapSingle(
            new CommonGetFolderResponse
            {
                Result = 1,
                AryEventfolderDatas =
                [
                    new EventFolderData { FolderId = 7, SongNoes = [101, 102] },
                    new EventFolderData { FolderId = 9, SongNoes = [201] }
                ]
            },
            requestedFolderId: 7);

        Assert.Equal(1u, response.Result);
        Assert.Equal(7u, response.FolderId);
        Assert.Equal([101u, 102u], response.SongNoes);

        var fields = ReadTopLevelFieldNumbers(Serialize(response));
        Assert.Contains(2, fields);
        Assert.Contains(3, fields);
    }

    [Fact]
    public void Taikojuku_FinalRouteReturnsEmptySuccess()
    {
        var response = Invoke(
            new FinalControllers.TaikojukuController(),
            controller => controller.FinalTaikojuku(new FinalWire.TaikojukuRequest
            {
                ChassisId = "268410000000",
                ShopId = "JPN0JPN0123",
                GetDans = [1]
            }),
            value => Assert.IsType<FinalWire.TaikojukuResponse>(value));

        Assert.Equal(1u, response.Result);
        Assert.Empty(response.AryJukupackDatas);
    }

    [Fact]
    public async Task PlayResult_FinalDanModeReturnsSuccessWithoutMediator()
    {
        var response = await InvokeAsync(
            new FinalControllers.PlayResultController(),
            controller => controller.FinalPlayResult(new FinalWire.PlayResultRequest
            {
                Baid = 1,
                ChassisId = "268410000000",
                ShopId = "JPN0JPN0123",
                PlayDatetime = "20260623090000",
                PlayMode = (uint)PlayMode.DanMode
            }),
            value => Assert.IsType<FinalWire.PlayResultResponse>(value));

        Assert.Equal(1u, response.Result);
    }

    private static TResponse Invoke<TController, TResponse>(
        TController controller,
        Func<TController, IActionResult> action,
        Func<object?, TResponse> assertResponse)
        where TController : ControllerBase
    {
        ConfigureController(controller);
        var ok = Assert.IsType<OkObjectResult>(action(controller));
        return assertResponse(ok.Value);
    }

    private static async Task<TResponse> InvokeAsync<TController, TResponse>(
        TController controller,
        Func<TController, Task<IActionResult>> action,
        Func<object?, TResponse> assertResponse)
        where TController : ControllerBase
    {
        ConfigureController(controller);
        var ok = Assert.IsType<OkObjectResult>(await action(controller));
        return assertResponse(ok.Value);
    }

    private static void ConfigureController(ControllerBase controller)
        => controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                RequestServices = new ServiceCollection()
                    .AddLogging()
                    .BuildServiceProvider()
            }
        };

    private static byte[] Serialize<T>(T value)
    {
        using var stream = new MemoryStream();
        Serializer.Serialize(stream, value);
        return stream.ToArray();
    }

    private static List<int> ReadTopLevelFieldNumbers(byte[] protobuf)
    {
        var fields = new List<int>();
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
