using TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15MusicInfoLoaderTests
{
    [Theory]
    [InlineData("J-POP", SongGenre.Pop)]
    [InlineData("アニメ", SongGenre.Anime)]
    [InlineData("童謡", SongGenre.Kids)]
    [InlineData("ボーカロイド", SongGenre.Vocaloid)]
    [InlineData("ゲームミュージック", SongGenre.GameMusic)]
    [InlineData("ナムコオリジナル", SongGenre.NamcoOriginal)]
    [InlineData("バラエティ", SongGenre.Variety)]
    [InlineData("クラシック", SongGenre.Classical)]
    public async Task MusicInfoLoader_MapsAc15GenreNameToCategoryId(string genreName, SongGenre expectedGenre)
    {
        var path = await WriteMusicInfoAsync(genreName);

        try
        {
            var result = await GreenMusicInfoLoader.LoadFromFileAsync(path, CancellationToken.None);

            var entry = Assert.Single(result.Entries);
            Assert.Equal((uint)expectedGenre, entry.CategoryId);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static async Task<string> WriteMusicInfoAsync(string genreName)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xml");
        await File.WriteAllTextAsync(
            path,
            $$"""
              <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
              <boost_serialization>
                <MusicInfo>
                  <Header>
                    <version>1</version>
                  </Header>
                  <Data>
                    <musicid>test_song</musicid>
                    <uniqueid>101</uniqueid>
                    <musicname>Test Song</musicname>
                    <genrename>{{genreName}}</genrename>
                    <tag>0</tag>
                  </Data>
                </MusicInfo>
              </boost_serialization>
              """,
            CancellationToken.None);

        return path;
    }
}
