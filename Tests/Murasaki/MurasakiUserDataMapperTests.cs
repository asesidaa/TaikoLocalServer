using TaikoLocalServer.Adapters.GameProtocol.Murasaki.Mappers;
using TaikoLocalServer.Adapters.GameProtocol.Murasaki.Wire;

namespace TaikoLocalServer.Tests.Murasaki;

public sealed class MurasakiUserDataMapperTests
{
    [Theory]
    [InlineData(2u, 2u)]
    [InlineData(0u, 1u)]
    [InlineData(26u, 1u)]
    public void UserData_SerializesDispTaikojukuDan(uint displayDan, uint expected)
    {
        var response = new UserDataResponse { Result = 1 };

        UserDataMappers.Apply(
            new Ac15UserDataDisplaySettings { DispTaikojukuDan = displayDan },
            response);

        Assert.True(response.ShouldSerializeDispTaikojukuDan());
        Assert.Equal(expected, response.DispTaikojukuDan);
    }
}
