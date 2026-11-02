using System.Buffers.Binary;
using System.Text;
using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15CatalogLoaderTests
{
    [Fact]
    public async Task MusicInfoLoader_ReadsBoostXmlVersionAndRows()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xml");
        await File.WriteAllTextAsync(path, """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <boost_serialization signature="serialization::archive" version="10">
              <MusicInfo>
                <Header>
                  <signature>TaikoAC15 MusicInfo</signature>
                  <version>12345</version>
                  <size>1</size>
                </Header>
                <Data>
                  <musicid>song_a</musicid>
                  <uniqueid>100</uniqueid>
                  <newrelease>1</newrelease>
                  <secret>0</secret>
                  <papamama>0</papamama>
                  <hasextreme>1</hasextreme>
                  <partsset>taiko</partsset>
                  <wai2partsset>taiko</wai2partsset>
                  <musicname>Song A</musicname>
                  <genrename>J-POP</genrename>
                  <demoplay>2</demoplay>
                  <tag>7</tag>
                  <tag>8</tag>
                </Data>
              </MusicInfo>
            </boost_serialization>
            """);

        try
        {
            var result = await Ac15MusicInfoLoader.LoadFromFileAsync(path, CancellationToken.None);

            Assert.Equal(12345u, result.SongHashVersion);
            var entry = Assert.Single(result.Entries);
            Assert.Equal("song_a", entry.MusicId);
            Assert.Equal(100u, entry.SongNo);
            Assert.True(entry.HasExtreme);
            Assert.Equal(0, entry.FileOrder);
            Assert.Equal([7u, 8u], entry.Tags);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task TaikojukuLoader_ReadsBoostXmlPackAndConditions()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xml");
        await File.WriteAllTextAsync(path, """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <boost_serialization signature="serialization::archive" version="8">
              <MusicMedleyInfoHeader>
                <signature>TaikoAC15 MusicMedleyInfo</signature>
                <version>54321</version>
                <size>1</size>
              </MusicMedleyInfoHeader>
              <MusicMedleyInfoData>
                <uniqueid>20001</uniqueid>
                <medleyname>First Dan</medleyname>
                <difficulty>3</difficulty>
                <challengelv>1</challengelv>
                <Content>
                  <musicid>song_a</musicid>
                  <uniqueid>100</uniqueid>
                  <difficulty>0</difficulty>
                  <notes>74</notes>
                </Content>
                <Conditions>
                  <tamashii>9000</tamashii>
                  <hit_ryo>10</hit_ryo>
                  <hit_ka>20</hit_ka>
                  <hit_fuka>30</hit_fuka>
                  <combo>40</combo>
                  <hits>50</hits>
                  <score>60</score>
                  <renda>70</renda>
                </Conditions>
                <ExcellentConditions>
                  <tamashii>9500</tamashii>
                  <hits>80</hits>
                </ExcellentConditions>
              </MusicMedleyInfoData>
            </boost_serialization>
            """);

        try
        {
            var entries = await Ac15TaikojukuLoader.LoadFromFileAsync(path, CancellationToken.None);

            var entry = Assert.Single(entries);
            Assert.Equal(20001u, entry.UniqueId);
            Assert.Equal(1u, entry.ChallengeLevel);
            Assert.Equal(90u, entry.Conditions.SoulGauge);
            Assert.Equal(50u, entry.Conditions.TotalHitCount);
            Assert.Equal(95u, entry.ExcellentConditions.SoulGauge);
            Assert.Equal(80u, entry.ExcellentConditions.TotalHitCount);
            Assert.Equal(100u, Assert.Single(entry.Songs).SongNo);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task TaikojukuLoader_AppliesVerupSidecarByChallengeLevel()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xml");
        var verupPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(path, """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <boost_serialization signature="serialization::archive" version="8">
              <MusicMedleyInfoData>
                <uniqueid>20001</uniqueid>
                <medleyname>First Dan</medleyname>
                <difficulty>3</difficulty>
                <challengelv>1</challengelv>
                <Content>
                  <musicid>song_a</musicid>
                  <uniqueid>100</uniqueid>
                  <difficulty>0</difficulty>
                  <notes>74</notes>
                </Content>
              </MusicMedleyInfoData>
              <MusicMedleyInfoData>
                <uniqueid>20002</uniqueid>
                <medleyname>Second Dan</medleyname>
                <difficulty>4</difficulty>
                <challengelv>2</challengelv>
                <Content>
                  <musicid>song_b</musicid>
                  <uniqueid>101</uniqueid>
                  <difficulty>1</difficulty>
                  <notes>80</notes>
                </Content>
              </MusicMedleyInfoData>
            </boost_serialization>
            """);
        await File.WriteAllTextAsync(verupPath, """
            {
              "defaultVerupNo": 5,
              "packs": [
                { "challengeLevel": 2, "verupNo": 7 }
              ]
            }
            """);

        try
        {
            var entries = await Ac15TaikojukuLoader.LoadFromFileAsync(
                path,
                verupPath,
                "Test",
                CancellationToken.None);

            Assert.Collection(
                entries,
                entry => Assert.Equal(5u, entry.VerupNo),
                entry => Assert.Equal(7u, entry.VerupNo));
        }
        finally
        {
            File.Delete(path);
            File.Delete(verupPath);
        }
    }

    [Fact]
    public async Task TuningLoader_ReadsHeaderCountAndExRecords()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.bin");
        await File.WriteAllBytesAsync(
            path,
            CreateTuningBin(
                new TuningTestRecord("song_a", 1, 2, 3, 4),
                new TuningTestRecord("ex_song_a", 0, 0, 0, 5)),
            CancellationToken.None);

        try
        {
            var stars = await Ac15TuningLoader.LoadFromFileAsync(path, "Test", CancellationToken.None);

            var starSet = Assert.Contains("song_a", stars);
            Assert.Equal(new Ac15StarSet(1, 2, 3, 4, 5), starSet);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static byte[] CreateTuningBin(params TuningTestRecord[] records)
    {
        const int headerSize = 4;
        const int recordSize = 2_316;
        const int player0CellOffset = 0x10;
        const int difficultyCellStride = 0x80;

        var stringTableOffset = headerSize + records.Length * recordSize;
        var stringTableLength = records.Sum(record => Encoding.ASCII.GetByteCount(record.MusicId) + 1);
        var bytes = new byte[stringTableOffset + stringTableLength];

        BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(0, 4), (uint)records.Length);

        var musicIdOffset = 0;
        for (var index = 0; index < records.Length; index++)
        {
            var record = records[index];
            var recordOffset = headerSize + index * recordSize;
            BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(recordOffset, 4), (uint)musicIdOffset);
            WriteStar(bytes, recordOffset, 0, record.Easy);
            WriteStar(bytes, recordOffset, 1, record.Normal);
            WriteStar(bytes, recordOffset, 2, record.Hard);
            WriteStar(bytes, recordOffset, 3, record.Oni);

            var musicIdBytes = Encoding.ASCII.GetBytes(record.MusicId);
            musicIdBytes.CopyTo(bytes.AsSpan(stringTableOffset + musicIdOffset));
            musicIdOffset += musicIdBytes.Length + 1;
        }

        return bytes;

        static void WriteStar(byte[] bytes, int recordOffset, int difficultyIndex, byte value)
        {
            BinaryPrimitives.WriteUInt32BigEndian(
                bytes.AsSpan(recordOffset + player0CellOffset + difficultyIndex * difficultyCellStride, 4),
                value);
        }
    }

    private sealed record TuningTestRecord(
        string MusicId,
        byte Easy,
        byte Normal,
        byte Hard,
        byte Oni);
}
