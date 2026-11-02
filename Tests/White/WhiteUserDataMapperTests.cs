using FinalMappers = TaikoLocalServer.Adapters.GameProtocol.White.Mappers;
using FinalWire = TaikoLocalServer.Adapters.GameProtocol.White.Wire;
using LegacyMappers = TaikoLocalServer.Adapters.GameProtocol.White.LegacyWire;
using LegacyWire = TaikoLocalServer.Adapters.GameProtocol.White.LegacyWire;

namespace TaikoLocalServer.Tests.White;

public sealed class WhiteUserDataMapperTests
{
    [Theory]
    [InlineData(2u, 2u)]
    [InlineData(0u, 1u)]
    [InlineData(26u, 1u)]
    public void UserData_FinalSchemaSerializesDispTaikojukuDan(uint displayDan, uint expected)
    {
        var response = new FinalWire.UserDataResponse { Result = 1 };

        FinalMappers.UserDataMappers.Apply(
            new Ac15UserDataDisplaySettings { DispTaikojukuDan = displayDan },
            response);

        Assert.True(response.ShouldSerializeDispTaikojukuDan());
        Assert.Equal(expected, response.DispTaikojukuDan);
    }

    [Theory]
    [InlineData(2u, 2u)]
    [InlineData(0u, 1u)]
    [InlineData(26u, 1u)]
    public void UserData_LegacySchemaSerializesDispTaikojukuDan(uint displayDan, uint expected)
    {
        var response = new LegacyWire.UserDataResponse { Result = 1 };

        LegacyMappers.LegacyUserDataMappers.Apply(
            new Ac15UserDataDisplaySettings { DispTaikojukuDan = displayDan },
            response);

        Assert.True(response.ShouldSerializeDispTaikojukuDan());
        Assert.Equal(expected, response.DispTaikojukuDan);
    }
}
