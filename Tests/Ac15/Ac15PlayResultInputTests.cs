using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15PlayResultInputTests
{
    [Fact]
    public void Envelope_CanCarryOnlySupportedCapabilityFacts()
    {
        var envelope = new Ac15PlayResultEnvelope(
            Metadata: new Ac15PlayResultMetadata(
                Baid: 1,
                ChassisId: "268410000000",
                ShopId: "JPN0JPN0123",
                PlayDatetime: "20260613120000",
                IsRight: false,
                CardType: 1,
                IsTwoPlayers: false,
                PlayMode: (uint)PlayMode.Normal,
                AreaCode: 12,
                Reserved: [1, 2],
                Accesstoken: "",
                ContentInfo: [3, 4]),
            Profile: Ac15ProfileMutationFacts.Empty with
            {
                AreaCode = 12,
                GetDonmedal = 25,
                GetToneNoes = [4]
            },
            Normal: new Ac15NormalPlayResult(
                Stages:
                [
                    new Ac15StageResult
                    {
                        SongNo = 101,
                        Level = Difficulty.Easy,
                        StageMode = 0,
                        PlayResult = 2,
                        PlayScore = 765432,
                        IsFavorite = true,
                        IsRecent = true
                    }
                ]),
            Dani: null,
            Tokkun: null,
            BlueBattle: null,
            GreenGhost: null);

        Assert.Equal(1u, envelope.Metadata.Baid);
        Assert.Null(envelope.Tokkun);
        Assert.Null(envelope.BlueBattle);
        Assert.Single(envelope.Normal!.Stages);
        Assert.Equal([4u], envelope.Profile.GetToneNoes);
    }
}
