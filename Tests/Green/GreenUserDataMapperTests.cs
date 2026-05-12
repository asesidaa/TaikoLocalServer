using TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenUserDataMapperTests
{
    [Theory]
    [InlineData(0u)]
    [InlineData(26u)]
    [InlineData(20001u)]
    public void UserData_OmitsInvalidDispTaikojukuDan(uint dispTaikojukuDan)
    {
        var response = UserDataMappers.Map(new CommonUserDataResponse
        {
            Result = 1,
            DispTaikojukuDan = dispTaikojukuDan
        });

        Assert.False(response.ShouldSerializeDispTaikojukuDan());
    }

    [Fact]
    public void UserData_SerializesValidDispTaikojukuDanSlot()
    {
        var response = UserDataMappers.Map(new CommonUserDataResponse
        {
            Result = 1,
            DispTaikojukuDan = 1
        });

        Assert.True(response.ShouldSerializeDispTaikojukuDan());
        Assert.Equal((uint)1, response.DispTaikojukuDan);
    }
}
