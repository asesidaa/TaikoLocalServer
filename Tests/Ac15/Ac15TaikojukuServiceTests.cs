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
                Pack(1, 9, [Song(101, Difficulty.Easy), Song(102, Difficulty.Normal), Song(103, Difficulty.Hard)])
            ],
            musicFileOrder: [Music(101), Music(102), Music(103)],
            validSongNoes: [101, 102, 103],
            Ac15EraProfiles.Green.Limits);

        var pack = Assert.Single(response.Packs);
        Assert.Equal(1u, pack.GetDan);
        Assert.Equal(3, pack.Songs.Count);
        Assert.Equal([Difficulty.Easy, Difficulty.Normal, Difficulty.Hard], pack.Songs.Select(song => song.Level).ToArray());
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

    [Fact]
    public void BuildResponse_DropsInvalidPackSlotsAndCapsSongsAtTen()
    {
        var response = Ac15TaikojukuService.BuildResponse(
            requestedDans: [0, 1],
            packs:
            [
                Pack(0, 0, [Song(101, Difficulty.Easy)]),
                Pack(1, 9,
                [
                    Song(0, Difficulty.Easy),
                    Song(1024, Difficulty.Easy),
                    Song(101, Difficulty.None),
                    Song(101, Difficulty.Easy),
                    Song(102, Difficulty.Normal),
                    Song(103, Difficulty.Hard),
                    Song(104, Difficulty.Oni),
                    Song(105, Difficulty.UraOni),
                    Song(106, Difficulty.Easy),
                    Song(107, Difficulty.Normal),
                    Song(108, Difficulty.Hard),
                    Song(109, Difficulty.Oni),
                    Song(110, Difficulty.UraOni),
                    Song(111, Difficulty.Easy)
                ])
            ],
            musicFileOrder: [Music(101), Music(102), Music(103)],
            validSongNoes: [101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111],
            Ac15EraProfiles.Green.Limits);

        var pack = Assert.Single(response.Packs);
        Assert.Equal(1u, pack.GetDan);
        Assert.Equal(10, pack.Songs.Count);
        Assert.DoesNotContain(pack.Songs, song => song.SongNo is 0 or 1024);
        Assert.All(pack.Songs, song => Assert.True(Ac15Difficulty.IsInRange(
            song.Level,
            Ac15EraProfiles.Green.Limits.MinCourseLevel,
            Ac15EraProfiles.Green.Limits.MaxCourseLevel)));
    }

    private static Ac15TaikojukuEntry Pack(uint challengeLevel, uint verupNo, IReadOnlyList<Ac15TaikojukuSong> songs) => new()
    {
        UniqueId = 20000 + challengeLevel,
        ChallengeLevel = challengeLevel,
        VerupNo = verupNo,
        Songs = songs
    };

    private static Ac15TaikojukuSong Song(uint songNo, Difficulty level) => new()
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
