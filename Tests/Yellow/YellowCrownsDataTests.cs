using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf;
using TaikoLocalServer.Adapters.GameProtocol.Yellow.Controllers;
using TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers;
using TaikoLocalServer.Adapters.GameProtocol.Yellow.Wire;
using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowCrownsDataTests
{
    [Fact]
    public void CrownsDataMapper_Yellow_PacksSavedBestRowsAndIncludesShin()
    {
        var rows = new[]
        {
            new SongBestDatumYellow
            {
                Baid = 1,
                SongId = 101,
                Difficulty = Difficulty.Easy,
                IsShin = false,
                BestCrown = CrownType.Clear
            },
            new SongBestDatumYellow
            {
                Baid = 1,
                SongId = 101,
                Difficulty = Difficulty.Easy,
                IsShin = true,
                BestCrown = CrownType.Gold
            },
            new SongBestDatumYellow
            {
                Baid = 1,
                SongId = 999,
                Difficulty = Difficulty.Oni,
                IsShin = false,
                BestCrown = CrownType.Dondaful
            }
        };

        var packed = CrownsDataMappers.BuildRawInflatedBody(rows, new YellowHandlerFixture.TestYellowCatalog());

        Assert.Equal(Ac15EraProfiles.Yellow.Limits.CrownPackedBytes, packed.Length);
        Assert.Equal((ushort)0b0000000011, ReadTenBitValue(packed, 101));
        Assert.Equal(0, ReadTenBitValue(packed, 999));
    }

    [Fact]
    public void CrownsDataResponse_Yellow_Field3ContainsRawInflatedCrownBytes_NotGzip()
    {
        var rawInflated = new byte[Ac15EraProfiles.Yellow.Limits.CrownPackedBytes];
        rawInflated[0] = 0x5a;
        var response = new CrownsDataResponse
        {
            Result = 1,
            SongHashVer = 789,
            HashCrownFlg = rawInflated
        };

        using var stream = new MemoryStream();
        Serializer.Serialize(stream, response);
        var serialized = stream.ToArray();

        var field3 = ReadLengthDelimitedField(serialized, 3);
        Assert.Equal(rawInflated, field3);
        Assert.Equal(rawInflated.Length, field3.Length);
        Assert.False(field3.Length >= 2 && field3[0] == 0x1f && field3[1] == 0x8b);
    }

    [Fact]
    public async Task CrownsData_Yellow_ReturnsRawInflatedCrownTable()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.SongBestDataYellow.Add(new SongBestDatumYellow
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
        Assert.Equal(789u, response.SongHashVer);
        Assert.NotEmpty(response.HashCrownFlg);
        Assert.Equal(Ac15EraProfiles.Yellow.Limits.CrownPackedBytes, response.HashCrownFlg.Length);
        Assert.Equal((ushort)0b0000000010, ReadTenBitValue(response.HashCrownFlg, 101));
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

    private static byte[] ReadLengthDelimitedField(byte[] protobuf, int fieldNumber)
    {
        var offset = 0;
        while (offset < protobuf.Length)
        {
            var tag = ReadVarint(protobuf, ref offset);
            var currentField = (int)(tag >> 3);
            var wireType = (int)(tag & 0x07);
            if (wireType == 2)
            {
                var length = (int)ReadVarint(protobuf, ref offset);
                var value = protobuf[offset..(offset + length)];
                offset += length;
                if (currentField == fieldNumber)
                {
                    return value;
                }
            }
            else if (wireType == 0)
            {
                _ = ReadVarint(protobuf, ref offset);
            }
            else
            {
                throw new InvalidOperationException($"Unsupported wire type {wireType}.");
            }
        }

        throw new InvalidOperationException($"Field {fieldNumber} was not found.");
    }

    private static uint ReadVarint(byte[] source, ref int offset)
    {
        uint value = 0;
        var shift = 0;
        while (offset < source.Length)
        {
            var next = source[offset++];
            value |= (uint)(next & 0x7f) << shift;
            if ((next & 0x80) == 0)
            {
                return value;
            }

            shift += 7;
        }

        throw new InvalidOperationException("Malformed varint.");
    }
}
