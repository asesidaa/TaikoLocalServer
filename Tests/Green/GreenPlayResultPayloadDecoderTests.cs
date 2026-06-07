using ProtoBuf;
using TaikoLocalServer.Adapters.GameProtocol.Green;
using TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;
using TaikoLocalServer.Adapters.GameProtocol.Green.Wire;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenPlayResultPayloadDecoderTests
{
    [Fact]
    public void Decode_PlayResultPayload_DecodesGzipCompressedProtobuf()
    {
        var request = CreatePlayResultDataRequest();
        using var serialized = new MemoryStream();
        Serializer.Serialize(serialized, request);
        var compressed = CompressGzip(serialized.ToArray());

        var decoded = GreenPlayResultPayloadDecoder.Decode(compressed);

        Assert.Equal("gzip", decoded.Format);
        Assert.Equal((uint)2, decoded.Request.Baid);
        Assert.Equal("268410000000", decoded.Request.ChassisId);
        Assert.Equal("JPN0JPN0123", decoded.Request.ShopId);
        Assert.Single(decoded.Request.AryStageInfoes);
        Assert.Equal((uint)873, decoded.Request.AryStageInfoes[0].SongNo);
    }

    [Fact]
    public void PlayResultController_LogsDecodedWireAndMappedCommonDataAsDestructuredObjects()
    {
        var source = File.ReadAllText(Path.Combine(
            FindRepoRoot(),
            "Adapters.GameProtocol.Green",
            "Controllers",
            "PlayResultController.cs"));

        Assert.Contains("Green PlayResult received dump: wire={@Request} mapped_common={@Common}", source, StringComparison.Ordinal);
        Assert.DoesNotContain("BuildReceivedDump", source, StringComparison.Ordinal);
    }

    private static PlayResultDataRequest CreatePlayResultDataRequest()
    {
        var request = new PlayResultDataRequest
        {
            Baid = 2,
            ChassisId = "268410000000",
            ShopId = "JPN0JPN0123",
            PlayDatetime = "2026-05-13 02:02:26",
            IsRight = true,
            CardType = 0,
            IsTwoPlayers = false,
            BonusDailyFlg = false,
            BonusWeeklyFlg = false,
            BonusMonthlyFlg = false,
            GetDonmedal = 10,
            GetKatsumedal = 0,
            GenderType = 0,
            PlayerAge = 0,
            PlayMode = 0,
            AreaCode = 0,
            Reserved = []
        };

        request.AryStageInfoes.Add(new PlayResultDataRequest.StageData
        {
            SongNo = 873,
            Level = 1,
            PlayResult = 2,
            PlayScore = 765432,
            GoodCnt = 100,
            OkCnt = 20,
            NgCnt = 3,
            PoundCnt = 4,
            ComboCnt = 120,
            OptionFlg = [1, 2, 3],
            ToneFlg = [4],
            MusicCateg = 1,
            IsPushed = false,
            IsFavorite = false,
            IsRecent = true,
            IsPapamama = false,
            SelectedFolderId = 0,
            StarLevel = 5,
            SupportLevel = 0
        });

        return request;
    }

    private static byte[] CompressGzip(byte[] body)
    {
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionLevel.SmallestSize, leaveOpen: true))
        {
            gzip.Write(body, 0, body.Length);
        }

        return output.ToArray();
    }

    private static string FindRepoRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "TaikoLocalServer.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("Could not find TaikoLocalServer.slnx.");
    }
}
