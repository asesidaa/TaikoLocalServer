using ProtoBuf;
using TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;
using TaikoLocalServer.Adapters.GameProtocol.Green.Wire;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenUserDataMapperTests
{
    // disp_taikojuku_dan must always be serialized as a valid 1..25 slot.
    // The Green client reads message+0x31C unconditionally (no proto2 has_*()
    // gate); absent or 0 underflows Taikojuku_GetDanSlotSongRange and crashes.
    // sub_7FDFFC's own "no data" path uses 1 as the sentinel - we mirror it.
    [Theory]
    [InlineData(0u)]
    [InlineData(26u)]
    [InlineData(20001u)]
    public void UserData_FallsBackToSentinelOneForInvalidDispTaikojukuDan(uint dispTaikojukuDan)
    {
        var response = AssembleGreenUserDataResponse(new Ac15UserDataResponse
        {
            Result = 1,
            Display = new Ac15UserDataDisplaySettings { DispTaikojukuDan = dispTaikojukuDan }
        });

        Assert.True(response.ShouldSerializeDispTaikojukuDan());
        Assert.Equal(1u, response.DispTaikojukuDan);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(26u)]
    [InlineData(20001u)]
    public void UserData_SerializesSentinelOneOnWireForInvalidDispTaikojukuDan(uint dispTaikojukuDan)
    {
        var response = AssembleGreenUserDataResponse(new Ac15UserDataResponse
        {
            Result = 1,
            Display = new Ac15UserDataDisplaySettings { DispTaikojukuDan = dispTaikojukuDan }
        });

        var payload = Serialize(response);

        Assert.True(ContainsField(payload, 34));
    }

    [Fact]
    public void UserData_FallsBackToSentinelOneWhenDispTaikojukuDanAbsent()
    {
        var response = AssembleGreenUserDataResponse(new Ac15UserDataResponse
        {
            Result = 1,
            Display = new Ac15UserDataDisplaySettings()
        });

        Assert.True(response.ShouldSerializeDispTaikojukuDan());
        Assert.Equal(1u, response.DispTaikojukuDan);
    }

    [Fact]
    public void UserData_SerializesValidDispTaikojukuDanSlot()
    {
        var response = AssembleGreenUserDataResponse(new Ac15UserDataResponse
        {
            Result = 1,
            Display = new Ac15UserDataDisplaySettings { DispTaikojukuDan = 7 }
        });

        Assert.True(response.ShouldSerializeDispTaikojukuDan());
        Assert.Equal(7u, response.DispTaikojukuDan);
    }

    [Fact]
    public void UserData_MapsGreenDisplayLevelFields()
    {
        var response = AssembleGreenUserDataResponse(new Ac15UserDataResponse
        {
            Result = 1,
            Display = new Ac15UserDataDisplaySettings
            {
                DispLevelTotal = 2,
                DispLevelChassis = 3,
                DispScoreType = 2,
                DispLevelSelf = 4
            }
        });

        Assert.True(response.ShouldSerializeDispLevelTotal());
        Assert.True(response.ShouldSerializeDispLevelChassis());
        Assert.True(response.ShouldSerializeDispScoreType());
        Assert.True(response.ShouldSerializeDispLevelSelf());
        Assert.Equal(2u, response.DispLevelTotal);
        Assert.Equal(3u, response.DispLevelChassis);
        Assert.Equal(2u, response.DispScoreType);
        Assert.Equal(4u, response.DispLevelSelf);
    }

    [Fact]
    public void UserData_MapsHowToPlayTutorialFlag()
    {
        var response = AssembleGreenUserDataResponse(new Ac15UserDataResponse
        {
            Result = 1,
            ModeFlags = new Ac15UserDataModeFlags(IsDevil: null, IsExplain: true)
        });

        Assert.True(response.ShouldSerializeIsExplain());
        Assert.True(response.IsExplain);
    }

    [Fact]
    public void UserData_SerializesToneAndTitleFlagsOnWire()
    {
        var response = AssembleGreenUserDataResponse(new Ac15UserDataResponse
        {
            Result = 1,
            SongFlags = new Ac15UserDataSongFlags
            {
                ToneFlg = BitsetCodec.Encode([0, 4], GreenProtocolBytes.ToneFlagBytes),
                TitleFlg = BitsetCodec.Encode([10, 131], GreenProtocolBytes.TitleFlagBytes)
            }
        });

        var payload = Serialize(response);

        Assert.True(response.ShouldSerializeToneFlg());
        Assert.True(response.ShouldSerializeTitleFlg());
        Assert.True(ContainsField(payload, 13));
        Assert.True(ContainsField(payload, 14));
    }

    private static UserDataResponse AssembleGreenUserDataResponse(Ac15UserDataResponse common)
    {
        var response = new UserDataResponse
        {
            Result = common.Result
        };

        UserDataMappers.Apply(common.SongFlags, response);
        UserDataMappers.Apply(common.SongLists, response);
        UserDataMappers.Apply(common.Recommendations, response);
        UserDataMappers.Apply(common.Counters, response);
        UserDataMappers.Apply(common.Display, response);

        if (common.ModeFlags is { } modeFlags)
        {
            UserDataMappers.Apply(modeFlags, response);
        }

        return response;
    }

    private static byte[] Serialize(UserDataResponse response)
    {
        using var stream = new MemoryStream();
        Serializer.Serialize(stream, response);
        return stream.ToArray();
    }

    private static bool ContainsField(byte[] payload, uint fieldNumber)
    {
        var index = 0;
        while (index < payload.Length)
        {
            var key = ReadVarint(payload, ref index);
            if ((key >> 3) == fieldNumber)
            {
                return true;
            }

            SkipValue(payload, ref index, (int)(key & 7));
        }

        return false;
    }

    private static ulong ReadVarint(byte[] payload, ref int index)
    {
        ulong value = 0;
        var shift = 0;
        while (index < payload.Length)
        {
            var b = payload[index++];
            value |= (ulong)(b & 0x7f) << shift;
            if ((b & 0x80) == 0)
            {
                return value;
            }

            shift += 7;
        }

        throw new InvalidOperationException("Unexpected end of protobuf varint.");
    }

    private static void SkipValue(byte[] payload, ref int index, int wireType)
    {
        switch (wireType)
        {
            case 0:
                _ = ReadVarint(payload, ref index);
                break;
            case 1:
                index += 8;
                break;
            case 2:
                var length = ReadVarint(payload, ref index);
                index += checked((int)length);
                break;
            case 5:
                index += 4;
                break;
            default:
                throw new InvalidOperationException($"Unsupported protobuf wire type {wireType}.");
        }
    }
}
