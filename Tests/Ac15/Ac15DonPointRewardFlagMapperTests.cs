using KimidoriMappers = TaikoLocalServer.Adapters.GameProtocol.Kimidori.Mappers.PlayResultMappers;
using KimidoriWire = TaikoLocalServer.Adapters.GameProtocol.Kimidori.Wire;
using MomoiroMappers = TaikoLocalServer.Adapters.GameProtocol.Momoiro.Mappers.PlayResultMappers;
using MomoiroWire = TaikoLocalServer.Adapters.GameProtocol.Momoiro.Wire;
using MurasakiMappers = TaikoLocalServer.Adapters.GameProtocol.Murasaki.Mappers.PlayResultMappers;
using MurasakiWire = TaikoLocalServer.Adapters.GameProtocol.Murasaki.Wire;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15DonPointRewardFlagMapperTests
{
    [Fact]
    public void MurasakiPlayResultMapper_DecodesRewardFlagBytes()
    {
        var limits = Ac15EraProfiles.Murasaki.Limits;
        var request = new MurasakiWire.PlayResultRequest
        {
            ToneFlg = BitsetCodec.Encode([4], limits.ToneFlagBytes),
            CostumeFlg1 = BitsetCodec.Encode([1], limits.CostumeFlagBytes),
            CostumeFlg2 = BitsetCodec.Encode([2], limits.CostumeFlagBytes),
            CostumeFlg3 = BitsetCodec.Encode([3], limits.CostumeFlagBytes),
            CostumeFlg4 = BitsetCodec.Encode([4], limits.CostumeFlagBytes),
            CostumeFlg5 = BitsetCodec.Encode([5], limits.CostumeFlagBytes),
            TitleFlg = BitsetCodec.Encode([10], limits.TitleFlagBytes)
        };

        var profile = MurasakiMappers.Map(request).Profile;

        Assert.Equal([4u], profile.GetToneNoes);
        Assert.Equal([1u], profile.GetCostumeNo1s);
        Assert.Equal([2u], profile.GetCostumeNo2s);
        Assert.Equal([3u], profile.GetCostumeNo3s);
        Assert.Equal([4u], profile.GetCostumeNo4s);
        Assert.Equal([5u], profile.GetCostumeNo5s);
        Assert.Equal([10u], profile.GetTitleNoes);
    }

    [Fact]
    public void KimidoriPlayResultMapper_DecodesRewardFlagBytes()
    {
        var limits = Ac15EraProfiles.Kimidori.Limits;
        var request = new KimidoriWire.PlayResultRequest
        {
            ToneFlg = BitsetCodec.Encode([4], limits.ToneFlagBytes),
            CostumeFlg1 = BitsetCodec.Encode([1], limits.CostumeFlagBytes),
            CostumeFlg2 = BitsetCodec.Encode([2], limits.CostumeFlagBytes),
            CostumeFlg3 = BitsetCodec.Encode([3], limits.CostumeFlagBytes),
            CostumeFlg4 = BitsetCodec.Encode([4], limits.CostumeFlagBytes),
            CostumeFlg5 = BitsetCodec.Encode([5], limits.CostumeFlagBytes),
            TitleFlg = BitsetCodec.Encode([10], limits.TitleFlagBytes)
        };

        var profile = KimidoriMappers.Map(request).Profile;

        Assert.Equal([4u], profile.GetToneNoes);
        Assert.Equal([1u], profile.GetCostumeNo1s);
        Assert.Equal([2u], profile.GetCostumeNo2s);
        Assert.Equal([3u], profile.GetCostumeNo3s);
        Assert.Equal([4u], profile.GetCostumeNo4s);
        Assert.Equal([5u], profile.GetCostumeNo5s);
        Assert.Equal([10u], profile.GetTitleNoes);
    }

    [Fact]
    public void MomoiroPlayResultMapper_DecodesRewardFlagBytes()
    {
        var limits = Ac15EraProfiles.Momoiro.Limits;
        var request = new MomoiroWire.PlayResultRequest
        {
            ToneFlg = BitsetCodec.Encode([4], limits.ToneFlagBytes),
            CostumeFlg1 = BitsetCodec.Encode([1], limits.CostumeFlagBytes),
            CostumeFlg2 = BitsetCodec.Encode([2], limits.CostumeFlagBytes),
            CostumeFlg3 = BitsetCodec.Encode([3], limits.CostumeFlagBytes),
            CostumeFlg4 = BitsetCodec.Encode([4], limits.CostumeFlagBytes),
            CostumeFlg5 = BitsetCodec.Encode([5], limits.CostumeFlagBytes),
            TitleFlg = BitsetCodec.Encode([10], limits.TitleFlagBytes)
        };

        var profile = MomoiroMappers.Map(request).Profile;

        Assert.Equal([4u], profile.GetToneNoes);
        Assert.Equal([1u], profile.GetCostumeNo1s);
        Assert.Equal([2u], profile.GetCostumeNo2s);
        Assert.Equal([3u], profile.GetCostumeNo3s);
        Assert.Equal([4u], profile.GetCostumeNo4s);
        Assert.Equal([5u], profile.GetCostumeNo5s);
        Assert.Equal([10u], profile.GetTitleNoes);
    }
}
