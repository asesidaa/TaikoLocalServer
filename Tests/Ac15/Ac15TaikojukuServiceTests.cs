using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15TaikojukuServiceTests
{
    [Fact]
    public void BuildResponse_ReturnsRequestedValidSlot()
    {
        var response = Ac15TaikojukuService.BuildResponse(
            requestedDans: [1],
            packs:
            [
                Pack(1, 9, [Song(101, 0), Song(102, 1), Song(103, 2)])
            ],
            musicFileOrder: [Music(101), Music(102), Music(103)],
            validSongNoes: [101, 102, 103],
            Ac15EraProfiles.Green.Limits);

        var pack = Assert.Single(response.Packs);
        Assert.Equal(1u, pack.GetDan);
        Assert.Equal(3, pack.Songs.Count);
        Assert.Equal(9u, pack.VerupNo);
    }

    [Fact]
    public void BuildResponse_AllInvalidSlotsFallbackIsCappedToEleven()
    {
        var response = Ac15TaikojukuService.BuildResponse(
            requestedDans: Enumerable.Range(101, 25).Select(value => (uint)value).ToArray(),
            packs: [],
            musicFileOrder: [Music(101), Music(102), Music(103), Music(104)],
            validSongNoes: [101, 102, 103, 104],
            Ac15EraProfiles.Green.Limits);

        Assert.Equal(11, response.Packs.Count);
        Assert.All(response.Packs, pack => Assert.InRange(pack.GetDan, 1u, 25u));
    }

    private static Ac15TaikojukuEntry Pack(uint challengeLevel, uint verupNo, IReadOnlyList<Ac15TaikojukuSong> songs) => new()
    {
        UniqueId = 20000 + challengeLevel,
        ChallengeLevel = challengeLevel,
        VerupNo = verupNo,
        Songs = songs
    };

    private static Ac15TaikojukuSong Song(uint songNo, uint level) => new()
    {
        SongNo = songNo,
        Level = level,
        MusicId = songNo.ToString()
    };

    private static Ac15MusicInfoEntry Music(uint songNo) => new()
    {
        SongNo = songNo,
        MusicId = songNo.ToString()
    };
}
