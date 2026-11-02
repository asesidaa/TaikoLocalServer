using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf;
using FinalControllers = TaikoLocalServer.Adapters.GameProtocol.White.Controllers;
using FinalWire = TaikoLocalServer.Adapters.GameProtocol.White.Wire;
using LegacyWire = TaikoLocalServer.Adapters.GameProtocol.White.LegacyWire;

namespace TaikoLocalServer.Tests.White;

public sealed class WhiteProtocolVersionCompatibilityTests
{
    [Fact]
    public void Heartbeat_CompatibilitySchemaOmitsFinalBanacoinFields()
    {
        var response = Invoke(
            new FinalControllers.HeartbeatController(),
            controller => controller.LegacyHeartbeat(new LegacyWire.HeartBeatRequest
            {
                ChassisId = "268410000000",
                ShopId = "JPN0JPN0123"
            }),
            value => Assert.IsType<LegacyWire.HeartBeatResponse>(value));

        var payload = Serialize(response);

        Assert.Equal(1u, response.Result);
        Assert.Equal(1u, response.ComSvrStat);
        Assert.Equal(1u, response.GameSvrStat);
        Assert.DoesNotContain(4, ReadFieldNumbers(payload));
        Assert.DoesNotContain(5, ReadFieldNumbers(payload));
    }

    [Fact]
    public void Heartbeat_FinalSchemaIncludesBanacoinStatusFields()
    {
        var response = Invoke(
            new FinalControllers.HeartbeatController(),
            controller => controller.Heartbeat(new FinalWire.HeartBeatRequest
            {
                ChassisId = "268410000000",
                ShopId = "JPN0JPN0123"
            }),
            value => Assert.IsType<FinalWire.HeartBeatResponse>(value));

        var payload = Serialize(response);

        Assert.Equal(1u, response.Result);
        Assert.Equal(1u, response.ComSvrStat);
        Assert.Equal(1u, response.GameSvrStat);
        Assert.Equal(1u, response.BnidSvrStat);
        Assert.Equal(1u, response.BanacoinStat);
        Assert.Contains(4, ReadFieldNumbers(payload));
        Assert.Contains(5, ReadFieldNumbers(payload));
    }

    private static TResponse Invoke<TController, TResponse>(
        TController controller,
        Func<TController, IActionResult> action,
        Func<object?, TResponse> assertResponse)
        where TController : ControllerBase
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                RequestServices = new ServiceCollection()
                    .AddLogging()
                    .BuildServiceProvider()
            }
        };

        var ok = Assert.IsType<OkObjectResult>(action(controller));
        return assertResponse(ok.Value);
    }

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
